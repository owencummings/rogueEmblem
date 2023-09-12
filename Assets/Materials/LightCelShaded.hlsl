#ifndef LIGHTING_CEL_SHADED_INCLUDED
#define LIGHTING_CEL_SHADED_INCLUDED
#endif

#if defined(UNITY_COMPILER_HLSL)
#define UNITY_INITIALIZE_OUTPUT(type,name) name = (type)0;
#else
#define UNITY_INITIALIZE_OUTPUT(type,name)
#endif

#ifndef SHADERGRAPH_PREVIEW
struct SurfaceVariables {
    float3 normal;
    float3 view;
    float smoothness;
    float shininess;
    float rimThreshold;
};

float3 CalculateCelShading(Light l, SurfaceVariables s) {
    float diffuse = saturate(dot(s.normal, l.direction));
    diffuse *= l.shadowAttenuation * l.distanceAttenuation;
    diffuse = diffuse > 0 ? 1 : 0;

    float diffuse2 = saturate(dot(s.normal, l.direction));
    diffuse2 *= l.shadowAttenuation * l.distanceAttenuation;
    diffuse2 = diffuse2 > 0.5 ? 1 : 0;



    float h = SafeNormalize(l.direction + s.view);
    float specular = saturate(dot(s.normal, h));    
    specular = pow(specular, s.shininess);
    specular *= diffuse2; // This ends up being 1 or zero, so just an existence check
    specular = specular > 0.1 ? 1 : 0;


    float rim = 1 - dot(s.view, s.normal);
    float magnitude = 1.5;
    //rim *= pow(diffuse, s.rimThreshold);
    //rim = rim > 0.75 ? 1 : 0;

    return (diffuse +  diffuse2 + specular) * magnitude/3;
}
#endif


void LightCelShaded_float(float3 Normal, float Smoothness, float3 View, float3 Position,
                            float RimThreshold, out float3 Color) {
#ifdef SHADERGRAPH_PREVIEW
    UNITY_INITIALIZE_OUTPUT(float3, Color);
#else
    UNITY_INITIALIZE_OUTPUT(float3, Color);
    SurfaceVariables s;
    s.view = SafeNormalize(View);
    s.smoothness = Smoothness;
    s.shininess = exp2(10 * Smoothness + 1);
    s.normal = normalize(Normal);
    s.rimThreshold = RimThreshold;

#if SHADOWS_SCREEN
    float4 clipPos = TransformWorldToHClip(Position);
    float4 shadowCoord = ComputeScreenPos(clipPos);
#else
    float4 shadowCoord = TransformWorldToShadowCoord(Position);
#endif

    Light light = GetMainLight(shadowCoord);
    Color = CalculateCelShading(light, s);

    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; i++){
        light = GetAdditionalLight(i, Position, 1);
        Color += CalculateCelShading(light, s);
    }

    //return Color;
#endif
}