# TFSToGit
 When migrating Teamproject from TFS to Git we may need to clean some files from the solution that is tied to TFS.
 We need to remove the TFS source control bindings from solution by deleting all of the *.vssscc and *.vspscc files
 and edit .sln file and remove the GlobalSection(TeamFoundationVersionControl) ... EndGlobalSection 
