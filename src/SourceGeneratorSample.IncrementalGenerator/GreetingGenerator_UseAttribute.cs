﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGeneratorSample.Core;
using System.Text;

namespace SourceGeneratorSample.IncrementalGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class GreetingGenerator_UseAttribute : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}
        context.RegisterSourceOutput(context.SyntaxProvider.ForAttributeWithMetadataName("SourceGeneratorSample.SayHello2Attribute"
                ,
                static (node, _) => node is MethodDeclarationSyntax
                {
                    Modifiers: var modifiers and not [],
                    Parent: TypeDeclarationSyntax
                    {
                        Modifiers: var typeModifiers and not []
                    }
                } &&
                                    modifiers.Any(SyntaxKind.PartialKeyword) &&
                                    typeModifiers.Any(SyntaxKind.PartialKeyword),
                (gasc, _) =>
                {
                    if (gasc is not
                        {
                            TargetNode: MethodDeclarationSyntax node,
                            TargetSymbol: IMethodSymbol
                            {
                                Name: var methodName,
                                TypeParameters: [],
                                Parameters:
                                [
                                    {
                                        Type.SpecialType: SpecialType.System_String,
                                        Name: var parameterName
                                    }
                                ],
                                ReturnsVoid: true,
                                IsStatic: true,
                                ContainingType:
                                {
                                    Name: var typeName,
                                    ContainingNamespace: var @namespace,
                                    TypeKind: var typeKind and (TypeKind.Class
                                    or TypeKind.Struct
                                    or TypeKind.Interface)
                                },
                                IsDefinition: true //是否声明
                            }
                        })
                    {
                        return null;
                    }
                    return new GatheredData(methodName, parameterName, typeName, @namespace, typeKind, node);
                }).Collect(),
            (spc, data) =>
            {
                foreach (var item in data)
                {
                    if (item is not var (methodName, parameterName, typeName, @namespace, typeKind, node))
                    {
                        continue;
                    }
                    var tyepKindString = typeKind switch
                    {
                        TypeKind.Class => "class",
                        TypeKind.Struct => "struct",
                        TypeKind.Interface => "interface"
                    };

                    //ClassDeclarationSyntax
                    var namespaceString = @namespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    namespaceString = namespaceString["global::".Length..]?.TrimEnd('>'); //global::
                    var output = $@"
// <auto-generated/>  
#nullable enable

namespace {namespaceString};


static partial {tyepKindString}  {typeName} 
                    {{

    {node.Modifiers}  void {methodName}(string {parameterName})
    {{
       global::System.Console.WriteLine($""SayHello2Attribute: '{{{parameterName}}}'"");
    }}
}}
";
                    var extensionTextFormatted = CSharpSyntaxTree.ParseText(output).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
                    spc.AddSource($"{typeName}{SourceGeneratorFileNameShortcut.GreetingGenerator_SayHello2Attribute_UseIncrementalGenerator}", SourceText.From(extensionTextFormatted, Encoding.UTF8));
                }
            });
    }
}

file sealed record GatheredData(string MethodName, string ParameterName, string TypeName, INamespaceSymbol Namespace, TypeKind TypeKind, MethodDeclarationSyntax Node);