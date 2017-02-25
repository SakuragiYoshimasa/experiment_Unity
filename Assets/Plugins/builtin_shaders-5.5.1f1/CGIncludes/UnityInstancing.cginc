#ifndef UNITY_INSTANCING_INCLUDED
#define UNITY_INSTANCING_INCLUDED

#ifndef UNITY_SHADER_VARIABLES_INCLUDED
	// We will redefine some built-in shader params e.g. unity_ObjectToWorld and unity_WorldToObject.
	#error "Please include UnityShaderVariables.cginc first."
#endif

#if SHADER_TARGET >= 35 && (defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_VULKAN) || (defined(SHADER_API_METAL) && defined(UNITY_COMPILER_HLSLCC)))
	#define UNITY_SUPPORT_INSTANCING
#endif

#if defined(SHADER_API_PSSL)
	#define UNITY_SUPPORT_INSTANCING
#endif

#if  (defined(STEREO_INSTANCING_ON) && defined(SHADER_API_D3D11)) ||  (defined(UNITY_SUPPORT_INSTANCING) && defined(INSTANCING_ON))

	#define UNITY_INSTANCING_ENABLED

	// A 'global' instance ID variable that functions can directly access.
	uint unity_InstanceID;

	CBUFFER_START(UnityDrawCallInfo)
		int unity_BaseInstanceID;	// Where the current batch starts within the instanced arrays.
		int unity_InstanceCount;	// Number of instances before doubling for stereo.
	CBUFFER_END

#if defined(SHADER_API_PSSL)
	#define UNITY_VERTEX_INPUT_INSTANCE_ID uint instanceID;
	#define UNITY_TRANSFER_INSTANCE_ID(input, output) output.instanceID = _GETINSTANCEID(input)
	#define UNITY_SETUP_INSTANCE_ID(input) unity_InstanceID = _GETINSTANCEID(input) + unity_BaseInstanceID
	#define UNITY_VERTEX_OUTPUT_STEREO
	#define UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output)
	#define UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input, output)
#else
	// Used in vertex shader input / output struct.
	#define UNITY_VERTEX_INPUT_INSTANCE_ID uint instanceID : SV_InstanceID;

	// Copy instance ID from input struct to output struct. Used in vertex shader.
	#define UNITY_TRANSFER_INSTANCE_ID(input, output) output.instanceID = input.instanceID

	// This should be used at the very beginning of the vertex shader / fragment shader, 
	// so that succeeding code can have access to the global unity_InstanceID.

	#if defined (SHADER_API_D3D11) && defined(STEREO_INSTANCING_ON)
		#define UNITY_SETUP_INSTANCE_ID(input) \
					unity_InstanceID = unity_BaseInstanceID + ((int)input.instanceID < unity_InstanceCount) ? input.instanceID : input.instanceID - unity_InstanceCount; \
					unity_StereoEyeIndex = ((int)input.instanceID < unity_InstanceCount) ? 0 : 1

		#define UNITY_VERTEX_OUTPUT_STEREO uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
		#define UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output) output.stereoTargetEyeIndex = unity_StereoEyeIndex
		#define UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input, output) output.stereoTargetEyeIndex = input.stereoTargetEyeIndex;
	#else
		#define UNITY_SETUP_INSTANCE_ID(input) unity_InstanceID = unity_BaseInstanceID + input.instanceID
		#define UNITY_VERTEX_OUTPUT_STEREO
		#define UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output)
		#define UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input, output) 
	#endif
#endif

	// The maximum number of instances a single instanced draw call can draw.
	// You can define your custom value before including this file.
	#ifndef UNITY_MAX_INSTANCE_COUNT
		#define UNITY_MAX_INSTANCE_COUNT 500
	#endif
	#if (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_METAL)) && !defined(UNITY_MAX_INSTANCE_COUNT_GL_SAME)
		// Many devices have max UBO size of 16kb
		#define UNITY_INSTANCED_ARRAY_SIZE (UNITY_MAX_INSTANCE_COUNT / 4)
	#else
		// On desktop, this assumes max UBO size of 64kb
		#define UNITY_INSTANCED_ARRAY_SIZE UNITY_MAX_INSTANCE_COUNT
	#endif

	// Every per-instance property must be defined in a specially named constant buffer.
	// Use this pair of macros to define such constant buffers.

	#if defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_METAL)
		// GLCore and ES3 have constant buffers disabled normally, but not here.
		#define UNITY_INSTANCING_CBUFFER_START(name)	cbuffer UnityInstancing_##name {
		#define UNITY_INSTANCING_CBUFFER_END			}
	#else
		#define UNITY_INSTANCING_CBUFFER_START(name)	CBUFFER_START(UnityInstancing_##name)
		#define UNITY_INSTANCING_CBUFFER_END			CBUFFER_END
	#endif

	// Define a per-instance shader property. Must be used inside a UNITY_INSTANCING_CBUFFER_START / END block.
	#define UNITY_DEFINE_INSTANCED_PROP(type, name)	type name[UNITY_INSTANCED_ARRAY_SIZE];

	// Access a per-instance shader property.
	#define UNITY_ACCESS_INSTANCED_PROP(name)		name[unity_InstanceID]

	// Redefine some of the built-in variables / macros to make them work with instancing.
	UNITY_INSTANCING_CBUFFER_START(PerDraw0)
		float4x4 unity_ObjectToWorldArray[UNITY_INSTANCED_ARRAY_SIZE];
		float4x4 unity_WorldToObjectArray[UNITY_INSTANCED_ARRAY_SIZE];
	UNITY_INSTANCING_CBUFFER_END

#if defined(UNITY_SUPPORT_INSTANCING) && defined(INSTANCING_ON)
	#define unity_ObjectToWorld		unity_ObjectToWorldArray[unity_InstanceID]
	#define unity_WorldToObject		unity_WorldToObjectArray[unity_InstanceID]

	#ifdef UNITY_INSTANCED_LOD_FADE
		// the quantized fade value (unity_LODFade.y) is automatically used for cross-fading instances
		UNITY_INSTANCING_CBUFFER_START(PerDraw1)
			float unity_LODFadeArray[UNITY_INSTANCED_ARRAY_SIZE];
		UNITY_INSTANCING_CBUFFER_END
		#define unity_LODFade unity_LODFadeArray[unity_InstanceID].xxxx
	#endif
#endif

#else

	#define UNITY_VERTEX_INPUT_INSTANCE_ID
	#define UNITY_TRANSFER_INSTANCE_ID(input, output)
	#define UNITY_SETUP_INSTANCE_ID(input)
	#define UNITY_VERTEX_OUTPUT_STEREO
	#define UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output)
	#define UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input, output) 

	#ifdef UNITY_MAX_INSTANCE_COUNT
		#undef UNITY_MAX_INSTANCE_COUNT
	#endif

	#define UNITY_INSTANCING_CBUFFER_START(name)	CBUFFER_START(name)
	#define UNITY_INSTANCING_CBUFFER_END			CBUFFER_END

	#define UNITY_DEFINE_INSTANCED_PROP(type, name)	type name;
	#define UNITY_ACCESS_INSTANCED_PROP(name)		name

#endif // UNITY_SUPPORT_INSTANCING && defined(INSTANCING_ON)

#endif // UNITY_INSTANCING_INCLUDED
