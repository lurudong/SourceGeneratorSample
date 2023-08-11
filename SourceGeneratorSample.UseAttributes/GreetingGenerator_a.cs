﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace SourceGeneratorSample.UseAttributes
{
    [Generator(LanguageNames.CSharp)]
    public sealed class GreetingGenerator_a : ISourceGenerator
    {
        ///<inheritodc/> 
        public void Execute(GeneratorExecutionContext context)
        {
            //var syntaxReceiver = (SyntaxContextReceiver)context.SyntaxContextReceiver!;

            if (context is not
                {
                    SyntaxContextReceiver: SyntaxContextReceiver
                    {
                        FoundSymbolPairs: var foundSymbolPairs
                    }
                })
            {
                return;
            }


            foreach (var foundSymbol in foundSymbolPairs)
            {

                var containingType = foundSymbol.ContainingType;
                var @namespace = containingType.ContainingNamespace;
                var classDeclaration = containingType.DeclaringSyntaxReferences;
                var classSyntaxNode = (ClassDeclarationSyntax)classDeclaration.FirstOrDefault().GetSyntax();

                //ClassDeclarationSyntax

                var namespaceString = @namespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                namespaceString = namespaceString["global::".Length..]; //global::

                if (foundSymbol.DeclaringSyntaxReferences is not
                    [var syntaxReference, ..])
                {
                    return;
                }

                //var cl = (ClassDeclarationSyntax)syntaxReference.GetSyntax();
                var syntaxNode = (MethodDeclarationSyntax)syntaxReference.GetSyntax();
                var tyepKindString = containingType.TypeKind switch
                {
                    TypeKind.Class => "class",
                    TypeKind.Struct => "struct"

                };

                var output = $@"
// <auto-generated/>  
#nullable enable

namespace {namespaceString};


{classSyntaxNode.Modifiers} {tyepKindString}  {containingType.Name} 
{{

    {syntaxNode.Modifiers}  void {foundSymbol.Name}(string name)
    {{
       global::System.Console.WriteLine($""GreetingUseUseAttributes: '{{name}}'"");
    }}
}}
";

                //使用这个方式格式化代码，免得生成的格式太难看    
                //string extensionTextFormatted = CSharpSyntaxTree.ParseText(output, new CSharpParseOptions(LanguageVersion.CSharp10)).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
                // 添加到源代码，这样IDE才能感知
                //context.AddSource($"{containingType.Name}.g.cs", SourceText.From(extensionTextFormatted, Encoding.UTF8));
                //// 保存一下类名，避免重复生成
                //ClassName.Add(classDeclarationSyntax.Identifier.ValueText);


                context.AddSource($"{containingType.Name}g.cs", output);
            }

        }

        ///<inheritodc/> 
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(static () => new SyntaxContextReceiver());


        }
    }
}

file sealed class SyntaxContextReceiver : ISyntaxContextReceiver
{
    public List<IMethodSymbol> FoundSymbolPairs { get; } = new();
    ///<inheritodc/> 
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {

        //if (context.Node is not ClassDeclarationSyntax  //它必须是一个类型的定义
        //    { Modifiers: var modifiers1 and not [] })
        //{
        //    return;
        //}

        if (context is not
            {
                Node: MethodDeclarationSyntax
                {
                    Modifiers: var modifiers and not []
                } methodSyntax,
                SemanticModel: { Compilation: var compilation } semanticModel
            })
        {
            return;
        }

        if (!modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }
        //var semanticModel = context.SemanticModel;
        //var compilation = semanticModel.Compilation;
        //var attribute = compilation.GetTypeByMetadataName("SourceGeneratorSample.UseAttribute")!;
        var attribute = compilation.GetTypeByMetadataName("SourceGeneratorSample.UseAttribute")!;
        var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax)!; //定义成员

        if (!methodSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attribute)))
        {
            return;
        }


        if (methodSymbol is not
            {
                ReturnsVoid: true,
                Parameters: [
                    {
                        Type.SpecialType: SpecialType.System_String,
                    }]
            })
        {
            return;
        }

        var containingType = methodSymbol.ContainingType;


        FoundSymbolPairs.Add(methodSymbol);

    }
}