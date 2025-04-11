sealed partial class Build
{
   const string Version = "1.0.0";
   readonly AbsolutePath ArtifactsDirectory = RootDirectory / "output";

   protected override void OnBuildInitialized()
   {
      Configurations = new[]
      {
            "Release R24",
            "Release R23",
            "Release R22",
            "Release R21",
            "Release R20",
            "Release R19",
            "Release R18",
        };

      Bundles = new[]
      {
            Solution.Solution_Items.CreateStairDesign
        };

      InstallersMap = new()
        {
            {Solution.Installer,  Solution.Solution_Items.CreateStairDesign}
        };
   }
}