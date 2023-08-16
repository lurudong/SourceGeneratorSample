// See https://aka.ms/new-console-template for more information

//必须手动编辑以包含 OutputItemType
//和 ReferenceOutputAssembly
//属性。 有关 ProjectReference 的
//OutputItemType
//和 ReferenceOutputAssembly
//属性的更多信息
//https://learn.microsoft.com/zh-cn/visualstudio/msbuild/common-msbuild-project-items?view=vs-2022#projectreference

using SourceGeneratorSample;
using SourceGeneratorSample.Model;

Greeting.SayHelloTo("大黄瓜");
GreetingUsePartialClass.SayHelloTo("大黄瓜18CM");
GreetingGeneratorAtrribute.SayHiTo("大黄瓜真骚");
GreetingUseIncremental.SayHelloTo("大黄瓜真骚");

GreetingUseAttribute_IncrementalGenerator.SayHello("大黄瓜真骚");

var myTuple = new MyTuple<int, int>(1, 2);
var myTuple2 = new MyTuple<int, int>(1, 2);

var color = new Color(255, 255, 255, 255);

var (a, r, g, b) = color;

Console.WriteLine(myTuple == myTuple2);
Console.ReadLine();

/// <summary>
/// 分部类型
/// </summary>
public static partial class GreetingUsePartialClass
{


    public static partial void SayHelloTo(string name);
}

