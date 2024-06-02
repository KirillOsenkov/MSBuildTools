to test:

msbuild /r /bl /p:EmitCompilerGeneratedFiles=true

it will generate 
Consumer\obj\Debug\net472\generated\Generator\Generator.SourceGenerator\myGeneratedFile.g.cs
(needs /p:EmitCompilerGeneratedFiles=true to save it to disk during the build)
