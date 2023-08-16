using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoDeconstructMethodGenerator;


[Generator(LanguageNames.CSharp)]
public sealed class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        context.RegisterSourceOutput(context.SyntaxProvider.ForAttributeWithMetadataName("SourceGeneratorSample.Model.DeconstructAttribute", (_, _) => true, (gasc, _) =>
        {

            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}

            if (gasc is not
                {
                    TargetNode: MethodDeclarationSyntax
                    {

                        Modifiers: var methodModifiers and not [],
                        Identifier.ValueText: "Deconstruct",
                        Parent: TypeDeclarationSyntax
                        {

                            Modifiers: var typeModifiers and not [],
                        }
                    },
                    TargetSymbol: IMethodSymbol
                    {
                        Name: "Deconstruct",
                        ReturnsVoid: true,
                        Parameters: { Length: >= 2 } parameters,
                        IsGenericMethod: false,
                        ContainingType:
                        {
                            Name: var typeName,
                            IsRecord: var isRecord,
                            TypeKind: var typeKind,
                            ContainingNamespace: var @namespace
                        }

                    }
                })
            {
                return null;
            }

            if (!methodModifiers.Any(SyntaxKind.PartialKeyword)
       || !typeModifiers.Any(SyntaxKind.PartialKeyword))
            {
                return null;
            }
            if (!parameters.All(p => p.RefKind == RefKind.Out))
            {
                return null;
            }
            var namespaceString = @namespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            namespaceString = namespaceString["global::".Length..]?.TrimEnd('>'); //global::
            return new GeneratorData(typeName, isRecord, typeKind, namespaceString, parameters.ToArray(), methodModifiers);


        }).Collect(), (spc, data) =>
        {

            foreach (var (fileName, data1) in from tupe in data
                                              group tupe
                                              by
                                              $"{tupe.Namespace}{tupe.TypeName}"
                                 into @group
                                              let fileName = $"{@group.Key}.g.admg.cs"
                                              select (FileName: fileName, Data: @group.ToArray()))
            {


                var listOfCode = new List<string>();

                foreach (var (_, isRecord, _, _, parameters, syntaxes) in data1)
                {

                    var parametersString =
                    string.Join(", ",
                    parameters.Select(p => $"out {p.Type.ToDisplayString()} {p.Name}"));

                    var assigns = string.Join(Environment.NewLine,
                        from parameter in parameters
                        let parameterName = parameter.Name
                        let pascalParameterName = parameterName.ToPascalString()
                        let method = (IMethodSymbol)parameter.ContainingSymbol
                        let type = method.ContainingType
                        let properties = type.GetMembers().OfType<IPropertySymbol>()
                        let foundProperty = properties.First(p => p.Name == pascalParameterName && SymbolEqualityComparer.Default.Equals(p.Type, parameter.Type))
                        select $"{parameterName} = {foundProperty.Name};"

                        );
                    listOfCode.Add($@" 
             [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
              [global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{nameof(Generator)}"",""{SourceGenerator.SourceGeneratorVersion.Value}"")]
              {syntaxes.ToString()} void Deconstruct({parametersString}){{

                {assigns}
}}
                    ");
                }

                var typeKindString = data1[0] switch
                {

                    {

                        IsRecord: true,
                        TypeKind: TypeKind.Class

                    } => "record",
                    {

                        IsRecord: true,
                        TypeKind: TypeKind.Struct

                    } => "record struct",
                    {

                        TypeKind: TypeKind.Class

                    } => "class",
                    {

                        TypeKind: TypeKind.Struct

                    } => "struct",
                    {

                        TypeKind: TypeKind.Interface

                    } => "interface",
                    _ => throw new NotSupportedException()

                };
                var output = @$"// <auto-generated/>  

                #nullable enable

                namespace {data1[0].Namespace};
             
                partial {typeKindString} {data1[0].TypeName}
                {{
                    {string.Join("\r\n\r\n\t", listOfCode)}
                }}
                    
                ";

                string extensionTextFormatted = CSharpSyntaxTree.ParseText(output, new CSharpParseOptions(LanguageVersion.CSharp11)).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();

                spc.AddSource(fileName, SourceText.From(extensionTextFormatted, Encoding.UTF8));

            }


        });
    }
}

file sealed record GeneratorData(string TypeName, bool IsRecord, TypeKind TypeKind, string Namespace, IParameterSymbol[] Parameters, SyntaxTokenList Syntaxes);

file static class Ext
{

    public static string ToPascalString(this string @this)
    {
        return $"{char.ToUpper(@this[0])}{@this[1..]}";
    }
}