using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TestPlace
{
    class Program
    {
        static void Main(string[] args)
        {

            // Use XMLReader if you don't want to load it into memory
            var document = XDocument.Load(@"D:\test.xml");
          var query = from element in document.Descendants("WorkItem") //Element("MyWorkTracker").Elements("WorkItems").Elements("WorkItem")
                        select element;

            foreach (var el2 in query)
            {
                Console.WriteLine($"Title = {el2.Element("Title").Value}");
                Console.WriteLine($"Description = {el2.Element("Description").Value}");
            }

/*            // Use XMLReader if you don't want to load it into memory
            var query = from element in document.Elements("MyWorkTracker") //.Elements("WorkItems").Elements("WorkItem")
                        select element;

            foreach (var el2 in query)
            {
                string version = el2.Attribute("ApplicationVersion").Value;
                string extractDate = el2.Attribute("ExtractDate").Value;
                Console.WriteLine($"{version}");
                Console.WriteLine($"{extractDate}");
            }*/



        }
    }
}
