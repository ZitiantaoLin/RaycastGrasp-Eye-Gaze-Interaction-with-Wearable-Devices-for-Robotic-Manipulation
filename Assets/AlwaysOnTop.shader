Shader "Custom/UnlitAlwaysOnTop"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,0,0,1)
    }
    SubShader
    {
        Tags {"Queue"="Overlay"}
        Lighting Off
        ZTest Always
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Color[_Color]
        }
    }
}
