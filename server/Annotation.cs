
namespace DocumentAnnotation {

    public class Annotation {
        public string Title {get;set;}
        public string Text  {get;set;}

        public List<Location> Marks {get;set;}
    }

    public class Location {
        public int BookNumber {get;set;}
        public int LineNumber {get;set;}
        public int Character {get;set;}
    }

}