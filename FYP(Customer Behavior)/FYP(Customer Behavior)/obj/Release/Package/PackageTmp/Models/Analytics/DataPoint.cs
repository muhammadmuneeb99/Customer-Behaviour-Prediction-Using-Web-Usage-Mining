using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Analytics
{
    [DataContract]
    public class DataPoint
    {

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "X")]
        public string X { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y")]
        public int Y { get;set; }

        [DataMember(Name = "X1")]
        public string X1 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y1")]
        public float Y1 { get; set; }

        [DataMember(Name = "X2")]
        public string X2 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y2")]
        public float Y2 { get; set; }

        [DataMember(Name = "X3")]
        public string X3 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y3")]
        public float Y3 { get; set; }

        /////For Admin Points//////////////////////////////////////////////////////////////////////////////////////////////////
        [DataMember(Name = "X4")]
        public string X4 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y4")]
        public int Y4 { get; set; }


        [DataMember(Name = "X5")]
        public string X5 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y5")]
        public int Y5 { get; set; }


        [DataMember(Name = "X6")]
        public string X6 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y6")]
        public int Y6 { get; set; }


        [DataMember(Name = "X7")]
        public string X7 { get; set; }
            
        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y7")]
        public int Y7 { get; set; }

        [DataMember(Name = "X8")]
        public string X8 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y8")]
        public float Y8 { get; set; }

        [DataMember(Name = "X9")]
        public string X9 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y9")]
        public int Y9 { get; set; }

        [DataMember(Name = "X10")]
        public string X10 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y10")]
        public int Y10 { get; set; }

        [DataMember(Name = "X11")]
        public string X11 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y11")]
        public int Y11 { get; set; }


        [DataMember(Name = "X12")]
        public string X12 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y12")]
        public float Y12 { get; set; }

        [DataMember(Name = "X13")]
        public string X13 { get; set; }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Y13")]
        public float Y13 { get; set; }

    }
}