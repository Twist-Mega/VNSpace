using System;
using System.Collections.Generic;
using System.Text;

namespace VNSpace
{
    class VisualNovel
    {
        private string name; //primary title
        private string company; //company that produced/developed the work
        private string writer; //writer that wrote scenario

        //the following is largely only relevant for VNDB linked VNs
        private string? vndbid; //string for vndb ID variable
        private string? coverArtURL; //change to Cover coverArt
        private float? averageRating;
        private int? voteCount; //number of votes
        private string? altName; //alternate name

        //constructor with vndb info
        public VisualNovel(string name, string company, string writer, string? coverArt, string? id){
            this.name = name;
            this.company = company;
            this.writer = writer;
            this.coverArtURL = coverArt;
            this.vndbid = id;
        }

        //constructor without vndb info
        public VisualNovel(string name, string company, string writer)
        {
            this.name = name;
            this.company = company;
            this.writer = writer;
        }

        //function to check if visual novel is linked to VNDB
        public bool IsLinkedToVNDB()
        {
            return vndbid != null;
        }
    }
}
