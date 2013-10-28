float4x4 World;
float4x4 View;
float4x4 Projection;

float4 LineColor = float4(0,0,0,1);
float LineThickness = 0.3;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;

};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Color = normalize(input.Normal * 1);

    return output;
}

VertexShaderOutput VertexShaderFunction2(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Color = normalize(input.Normal * -1);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return saturate(input.Color);
}

technique Test
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
    pass Pass2
    {
        VertexShader = compile vs_2_0 VertexShaderFunction2();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }



}
