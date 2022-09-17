ILMerge (two versions) and GUI, src and binaries.  
unofficial patched-up builds, Windows 11 compatible.  

using is as easy as `ilmerge.exe my.exe my.dll /out:combined.exe` .  

probably one useful usage is embedding Json.NET (`Newtonsoft.Json.dll`) .  

there are more complex ways of using it,  
as well as using Visual-Studio tasks, or build events and a batch file  
to automate it.  

- https://google.com/search?q=using+ilmerge+in+visual+studio  
- https://github.com/gzvulon/ILMerge-Example/blob/master/ILMerge/merge_all.bat  
- https://github.com/emerbrito/ILMerge-MSBuild-Task  
- https://github.com/wade/ILMerge.Tools  

