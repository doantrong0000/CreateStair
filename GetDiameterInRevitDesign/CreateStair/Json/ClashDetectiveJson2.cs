namespace CreateStairDesign.CreateStair.Json
{
    public class ClashDetectiveJson2
    {
        public List<ClashDetective> ClashDetectives { get; set; }
        public string Color1 { get; set; }
        public string Color2 { get; set; }
        public bool IsSortByTotalClash { get; set; }
    }

    public class ClashDetective
    {
        public string GroupName { get; set; }

        public List<ClashDetectiveInfo> ClashDetectiveInfos { get; set; }
    }

    public class ClashDetectiveInfo
    {
        public int STT { get; set; }
        public string Pattern { get; set; }
        public string Id1 { get; set; }
        public string Id2 { get; set; }
        public int TotalItem { get; set; }
        public string FileRevitName1 { get; set; }
        public string FileRevitName2 { get; set; }
        public string DisplayName1 { get; set; }
        public string DisplayName2 { get; set; }
        public string PathImage1 { get; set; }
        public string PathImage2 { get; set; }
        public string Diameter1 { get; set; }
        public double Diameter1Double { get; set; }
        public string Diameter2 { get; set; }
        public double Diameter2Double { get; set; }
        public bool IsRectangOverlap { get; set; }
    }
}
