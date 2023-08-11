// See https://aka.ms/new-console-template for more information

//必须手动编辑以包含 OutputItemType
//和 ReferenceOutputAssembly
//属性。 有关 ProjectReference 的
//OutputItemType
//和 ReferenceOutputAssembly
//属性的更多信息
//https://learn.microsoft.com/zh-cn/visualstudio/msbuild/common-msbuild-project-items?view=vs-2022#projectreference

using SourceGeneratorSample;

Greeting.SayHelloTo("大黄瓜");
GreetingUsePartialClass.SayHelloTo("大黄瓜18CM");
GreetingGeneratorAtrribute.SayHiTo("大黄瓜真骚");
Console.ReadLine();

/// <summary>
/// 分部类型
/// </summary>
public static partial class GreetingUsePartialClass
{


    public static partial void SayHelloTo(string name);
}