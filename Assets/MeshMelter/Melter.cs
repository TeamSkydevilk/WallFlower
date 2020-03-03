//
// Mesh Melter v1.0 - Sep, 2017 - by Pixeldust @ Cloverbit srl (pixeldust@cloverbit.com)
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD
// TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS.
// IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL
// DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN
// AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
// CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public delegate void MeltCallback(GameObject go);

namespace MeshMelter {
	
	public class Melter : MonoBehaviour {
		
		[Tooltip("When true, melting is ongoing")]
		[SerializeField] public bool active = false;
		
		[Tooltip("Current melting level")]
		[SerializeField] [Range(0f, 1f)] public float meltingLevel = 0;
		
		[Tooltip("Set shaders for all materials. When false, only meshes where the shader is already set will melt")]
		[SerializeField] public bool injectShaders = true;
		
		[Tooltip("Modify bounding box to prevent frustum culling")]
		[SerializeField] public bool preventCulling = false;
		
		[Tooltip("Melting time")]
		[SerializeField] public float duration = 10f;
		
		[Tooltip("Gradually apply this color to melting object")]
		[SerializeField] public Color meltTint = Color.white;
		
		[Tooltip("Gravity direction in WORLD COODINATES (green arrow in gizmos)")]
		[SerializeField] public Vector3 gravityDirection = -Vector3.up;
		
		[Tooltip("Offset of melting point from center as WORLD DISTANCE (red square in gizmos)")]
		[SerializeField] public float gravityOffset = -1f; // forces recalculation
		
		[Tooltip("Offset of the center with respect to the actual geometric center")]
		[SerializeField] public Vector2 centerOffset = Vector2.zero;
		
		[Tooltip("Increase meltdown for peripheral vertices")]
		[SerializeField] [Range(0f, 1f)] public float peripheralSpeedup = 0.2f;
		
		[Tooltip("Height of molten stack RELATIVE to the mesh height (yellow bar in gizmos)")]
		[SerializeField] [Range(0f, 0.5f)] public float stackHeight = 0.2f;
		
		[Tooltip("Radius of molten stack as WORLD DISTANCE (size of red square in gizmos)")]
		[SerializeField] public float stackRadius = -1f; // forces recalculation
		
		[Tooltip("How smooth will be the stack surface")]
		[SerializeField] [Range(0f, 1f)] public float stackSmoothness = 0.5f;
		
		[Tooltip("Spreading factor on the ground")]
		[SerializeField] [Range(0f, 1f)] public float spreadFactor = 0.5f;
		
		// SCRIPTING API
		
		// delegate to be called before starting the effect
		[HideInInspector] public MeltCallback StartCallback = null;
		
		// delegate to be called after finishing the effect
		[HideInInspector] public MeltCallback EndCallback = null;
		
		// shorthand list to quickly access all shaders with associated transform and mesh filter
		private MandT[] myMaterials;
		
		// used to detect if melting level has been changed from outside (script or interface)
		internal float oldMeltingLevel = 0f;
		
		private struct MandT {
			
			public Material material;
			public Transform transform;
			public Vector3 scaleFactor;

			public MandT(Material ma, Transform tr, Vector3 scale) {
				material = ma;
				transform = tr;
				scaleFactor = scale;
			}
		}
		
		// true when the effect has started, no matter if active or not
		internal bool started = false;
		
		// accounts elapsed time
		private float elapsedTime = 0f;
		
		// we give framerate priority to the game
		void LateUpdate() {
			
			// if we are active and the effect is not over
			if (active) {
				// at first iteration (when we enable the active flag)
				if (!started) {
					oldMeltingLevel = meltingLevel = 0f;
					// calling order is important!
					CalculateReferences (); // must be called before PreventCulling()
					if (!SetupMaterials()) { // SetupMaterials fails when the shader is not available, in this case we abort
						active = false;
						started = false;
						return;
					};
					PreventCulling();
					SetGeometryParameters(); // do not remove CalculateReferences() above or this calls will screw up
					SetInterfaceParameters();
					if (StartCallback != null) StartCallback(gameObject);
					started = true;
				}
				if (oldMeltingLevel == meltingLevel) {
					// if the melting level has not been modified by outside (script or interface) we just account for frame time
					elapsedTime += Time.deltaTime;
					oldMeltingLevel = meltingLevel = Mathf.Clamp ((elapsedTime / duration), 0f, 1f);
				} else {
					// otherwise, we set the melting to the required level
					elapsedTime = duration * meltingLevel;
					oldMeltingLevel = meltingLevel;
				}
				// set the "right" shape based on melting level
				SetShapeParamater();
				if (elapsedTime >= duration) {
					// if the elased time has reached duration we are done
					active = false;
					if (EndCallback != null) EndCallback(gameObject);
				}
			}
		}
		
		// reference fields
		protected Vector3 center = Vector3.zero;	// blue ball in gizmos
		protected float sqRadialExtension;			// to calculate waves per meter and have the perpherial vertices drop first  
		
		// offset is not enough if we want to calculate Hstack based on actual mesh height
		// baseOffset is where feets are: lowest point of the (original) bounding box along gravity direction 
		private float baseOffset = 0f;
		
		#if UNITY_EDITOR
		// internal gizmos fields
		protected Vector3 ground = Vector3.zero;	// red rectangle in gizmos
		private float meshHeight = 0f;		// estimated height of the mesh
		private float gizmosHeight = 2f;	// size reference for the gizmos
		#endif
		
		// calculate the geometric center, collapsing plane, and default distances
		private void CalculateReferences() {
			Bounds bounds = new Bounds (transform.position, Vector3.zero);
			foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
				// try to be safe from crappy imports
				if (r.bounds.extents != Vector3.zero) {
					bounds.Encapsulate (r.bounds);
				}
			}
			// the center is the center of the bounding box for all active renderers plus the required offset along the collapsing plane
			Matrix4x4 rotationMatrix = Matrix4x4.TRS (Vector3.zero, Quaternion.LookRotation (gravityDirection.normalized), Vector3.one);
			Vector3 adjustedCenter = bounds.center + rotationMatrix.MultiplyVector (new Vector3 (centerOffset.x, centerOffset.y, 0f));
			center = transform.InverseTransformPoint(adjustedCenter);
			// the base offset is the lowest point of the bounding box along gravity, but we cannot use adjusted center for that
			Ray x = new Ray(bounds.center, gravityDirection);
			bounds.IntersectRay (x, out baseOffset);
			baseOffset = Mathf.Abs(baseOffset);
			// default value for offset is the base offset
			if (gravityOffset < 0)
				gravityOffset = baseOffset;
			// default value for the stack radius is the minimum extent value of the bounding box
			if (stackRadius < 0)
				stackRadius = Mathf.Min (Mathf.Min (bounds.extents.x, bounds.extents.y), bounds.extents.z);
			sqRadialExtension = bounds.size.x * bounds.size.x + bounds.size.z * bounds.size.z;
			
			#if UNITY_EDITOR
			ground = transform.InverseTransformPoint (adjustedCenter + gravityDirection.normalized * gravityOffset);
			gizmosHeight = bounds.extents.magnitude * 1.5f;
			meshHeight = bounds.size.y;
			#endif
		}
		
		// name of the shader to inject
		private const string shaderName = "Cloverbit/MeshMelter";
		private const string stessellatinShaderName = "Cloverbit/MeshMelter";
		
		// clone all materials and inject shaders if required
		// this is to avoid inheriting transition state from other gameobjects using the same material
		// moreover, we do not want to change the material in the asset database
		private bool SetupMaterials () {
			
			Shader meltShader = null;
			if (injectShaders) {
				meltShader = Shader.Find (shaderName);
				if (meltShader == null) {
					Debug.LogError(shaderName + " is not available, aborting effect", gameObject);
					#if UNITY_EDITOR
					EditorUtility.DisplayDialog ("Mesh Melter", "ERROR!\n\n" +
					                             "The required shader (" + shaderName + ") is not available in the asset database. " +
					                             "Most probably, you deleted, renamed, or modified the source file.\n" +
					                             "Please, fix the situation and try again.\n", "I am sorry");
					#endif
					return false;
				}
			}
			
			Material m;
			List<MandT> ml = new List<MandT> ();
			
			foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer> ()) {
				if (mr.sharedMaterials != null) {
					Material[] clonedMatList = new Material[mr.sharedMaterials.Length];
					for (int i = 0; i < mr.sharedMaterials.Length; i += 1) {
						m = Instantiate<Material>(mr.sharedMaterials[i]);
						if (injectShaders) m.shader = meltShader;
						clonedMatList[i] = m;
						ml.Add (new MandT (m, mr.transform, Vector3.one));
					}
					mr.materials = clonedMatList;
				} else {
					Debug.LogWarning ("Material(s) not set in object " + mr.transform.name);
				}
			}
			
			foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer> ()) {
				if (smr.sharedMaterials != null) {
					Material[] cloneMatList = new Material[smr.sharedMaterials.Length];
					for (int i = 0; i < smr.sharedMaterials.Length; i += 1) {
						m = Instantiate<Material>(smr.sharedMaterials[i]);
						if (injectShaders) m.shader = meltShader;
						cloneMatList[i] = m;
						// center offset must be scaled by the (global) scale of rootbone described in rootbone local orientation 
						Vector3 scale = Vector3.one;
						Quaternion orientation = Quaternion.identity;
						Transform t = smr.rootBone;
						while (t != null) {
							scale = Vector3.Scale (scale, t.localRotation * t.localScale);
							orientation = t.localRotation * orientation;
							t = t.parent;
						}
						scale = orientation * scale;
						// and then we set the scale to absolute value to prevent mesh folding when rotating by -90 degrees
						scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
						ml.Add (new MandT (m, smr.rootBone, scale));
					}
					smr.materials = cloneMatList;
				} else {
					Debug.LogWarning ("Material(s) not set in object " + smr.transform.name);
				}
			}
			
			myMaterials = ml.ToArray ();
			return true;
		}
		
		// disable frustum culling
		// if we don't do this, the displaced mesh will disappear when the original vertices are all out of the camera frustum
		private void PreventCulling() {
			if (preventCulling) {
				foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>()) {
					Mesh m = mf.mesh;
					m.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f); // rule of thumb here, 1000 should be enough
					mf.mesh = m;
				}
			}
		}
		
		// set all shader paramaters not changing over time and depending on geometry
		private void SetGeometryParameters() {
			// north reference, to generate radial noise
			// must be random vector perpendicular to the gravity axis
			Vector3 nr = Vector3.Cross(gravityDirection, Random.onUnitSphere).normalized;
			foreach (MandT mt in myMaterials) {
				mt.material.SetVector("_NorthReference", nr);
				mt.material.SetFloat("_RHz", 2f / Mathf.Sqrt(sqRadialExtension));
				mt.material.SetFloat("_BaseOffset", baseOffset);
			}
		}

		// set all shader paramaters not changing over time and depending on component options
		private void SetInterfaceParameters() {
			foreach (MandT mt in myMaterials) {
				// technically, the center is a geometric parameter, but we must set it here as a result for changing centerOffset
				// the scale factor is different from Vector3.one only for skinned mesh renderers
				mt.material.SetVector ("_Center", Vector3.Scale(mt.transform.InverseTransformPoint (transform.TransformPoint (center)), mt.scaleFactor));
				mt.material.SetVector("_Gravity", gravityDirection); 
				mt.material.SetColor("_MeltTint", meltTint);
				mt.material.SetFloat("_Offset", gravityOffset);
				mt.material.SetFloat("_MeltAmplify", peripheralSpeedup);
				mt.material.SetFloat("_HStack", stackHeight);
				mt.material.SetFloat("_RStack", stackRadius);
				mt.material.SetFloat("_Smooth", stackSmoothness);
				mt.material.SetFloat("_Spread", spreadFactor);
			}
		}
		
		// set the shape based on melting level
		protected void SetShapeParamater() {
			SetDynamicParameter("_Melt", meltingLevel);
		}
		
		// set a shader paramater on all material
		private void SetDynamicParameter(string key, float value) {
			if (myMaterials == null) return;
			foreach (MandT mt in myMaterials) {
				mt.material.SetFloat(key, value);
			}
		}
		
		#if UNITY_EDITOR
		public void OnValidate() {
			// make sure some values are non negative
			duration = Mathf.Max(duration, 0f);
			gravityOffset = Mathf.Max (gravityOffset, 0f);	
			stackRadius = Mathf.Max (stackRadius, 0f);

			// re-calculate references in case some offsed are changed
			CalculateReferences ();

			// if we are also running, we must re-inject the static parameters
			if (started) {
				SetInterfaceParameters();
				
				// and the melting level if it has changed
				if (oldMeltingLevel != meltingLevel) {
					elapsedTime = duration * meltingLevel;
					oldMeltingLevel = meltingLevel;
					SetShapeParamater();
				}
			}
		}
		#endif

		#if UNITY_EDITOR
		// gizmos are drawn only when the object is selected
		void OnDrawGizmosSelected() {
			
			// drawing gizmos while enlarging bounds may geive weird results, so better switch off when playing
			if (!Application.isPlaying || !preventCulling) {

				// make sure references are set after reset
				if (gravityOffset < 0) CalculateReferences ();
				
				Vector3 normalizedGravity = gravityDirection.normalized;
				Vector3 p1 = transform.TransformPoint (center);
				Vector3 p2 = transform.TransformPoint (ground);
				
				// geometric center (blue ball)
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere (p1, gizmosHeight / 20f);
				
				Matrix4x4 rotationMatrix = Matrix4x4.TRS (Vector3.zero, Quaternion.LookRotation (normalizedGravity), Vector3.one);
				Gizmos.matrix = rotationMatrix;
				
				// gravity direction (green "arrow") and collapsing plane estimation (red rectangle)
				if (gravityDirection != Vector3.zero) {
					Gizmos.color = Color.red;
					float d = stackRadius * (1f + spreadFactor) * 2f; // must be coherent with the shader melt calculation
					Gizmos.DrawCube (rotationMatrix.inverse.MultiplyVector (p2), new Vector3 (d, d, 0.01f));
					Gizmos.color = Color.green;
					Gizmos.DrawCube (rotationMatrix.inverse.MultiplyVector (p1 + normalizedGravity * gizmosHeight / 6f), new Vector3 (gizmosHeight / 40f, gizmosHeight / 40f, gizmosHeight / 2.5f));
					Gizmos.DrawCube (rotationMatrix.inverse.MultiplyVector (p1 + normalizedGravity * gizmosHeight / 3.3f), new Vector3 (gizmosHeight / 20f, gizmosHeight / 20f, gizmosHeight * 0.05f));
					Gizmos.DrawCube (rotationMatrix.inverse.MultiplyVector (p1 + normalizedGravity * gizmosHeight / 3.39f), new Vector3 (gizmosHeight / 12.5f, gizmosHeight / 12.5f, gizmosHeight * 0.025f));
				}
				
				// stack height (yellow bar)
				if (stackRadius > 0f && stackHeight > 0f) {
					Gizmos.color = Color.yellow;
					Gizmos.DrawCube (rotationMatrix.inverse.MultiplyVector (p2 - normalizedGravity * (stackHeight * meshHeight / 2f)), new Vector3 (gizmosHeight / 40f, gizmosHeight / 40f, stackHeight * meshHeight));
					
				}
			}
		}
		#endif
	}		
	
	#if UNITY_EDITOR
	[CustomEditor(typeof(Melter), true)]
	[CanEditMultipleObjects]
	public class MelterInspectorPanel : Editor {
		
		Melter script;
		
		SerializedProperty active;
		SerializedProperty meltingLevel;
		SerializedProperty injectShaders;
		SerializedProperty preventCulling;
		SerializedProperty duration;
		SerializedProperty meltTint;
		SerializedProperty gravityDirection;
		SerializedProperty centerOffset;
		SerializedProperty gravityOffset;
		SerializedProperty peripheralSpeedup;
		SerializedProperty stackHeight;
		SerializedProperty stackRadius;
		SerializedProperty stackSmoothness;
		SerializedProperty spreadFactor;
		
		GUIStyle warningText;
		
		void OnEnable() {
			active				= serializedObject.FindProperty("active");
			meltingLevel		= serializedObject.FindProperty("meltingLevel");
			injectShaders		= serializedObject.FindProperty("injectShaders");
			preventCulling		= serializedObject.FindProperty("preventCulling");
			duration			= serializedObject.FindProperty("duration");
			meltTint			= serializedObject.FindProperty("meltTint");
			gravityDirection	= serializedObject.FindProperty("gravityDirection");
			gravityOffset		= serializedObject.FindProperty("gravityOffset");
			centerOffset		= serializedObject.FindProperty("centerOffset");
			peripheralSpeedup	= serializedObject.FindProperty("peripheralSpeedup");
			stackHeight			= serializedObject.FindProperty("stackHeight");
			stackRadius			= serializedObject.FindProperty("stackRadius");
			stackSmoothness		= serializedObject.FindProperty("stackSmoothness");
			spreadFactor 		= serializedObject.FindProperty ("spreadFactor");
			
			warningText = new GUIStyle ();
			warningText.normal.textColor = Color.red * Color.gray;
			warningText.fontSize = 11;
			warningText.margin = new RectOffset(26, 0, 0, 0);
		}
		
		override public void OnInspectorGUI() {
			serializedObject.Update ();
			EditorGUIUtility.wideMode = true;
			EditorGUILayout.PropertyField (active);
			// the following is just cosmetic, it avoids having a value grather than 0 for melting level when the effect is not yet started
			if (!((Melter)target).started) ((Melter)target).oldMeltingLevel = ((Melter)target).meltingLevel = 0f;
			EditorGUI.BeginDisabledGroup (!((Melter)target).started);
			EditorGUILayout.PropertyField (meltingLevel);
			EditorGUI.EndDisabledGroup ();
			EditorGUILayout.PropertyField (injectShaders);
			if (!((Melter)target).injectShaders)
				GUILayout.Label ("Remember to set shader manually!", warningText);
			EditorGUILayout.PropertyField (preventCulling);
			EditorGUILayout.PropertyField (duration);
			EditorGUILayout.PropertyField (meltTint);
			EditorGUILayout.PropertyField (gravityDirection);
			EditorGUILayout.PropertyField (gravityOffset);
			EditorGUILayout.PropertyField (centerOffset);
			EditorGUILayout.PropertyField (peripheralSpeedup);
			EditorGUILayout.PropertyField (stackHeight);
			EditorGUILayout.PropertyField (stackRadius);
			EditorGUILayout.PropertyField (stackSmoothness);
			EditorGUILayout.PropertyField (spreadFactor);
			serializedObject.ApplyModifiedProperties();
		}
	}
	#endif
	
}