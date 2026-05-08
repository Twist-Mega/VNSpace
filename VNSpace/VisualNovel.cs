using System;
using System.Collections.Generic;
using System.Text;

namespace VNSpace
{
    class VisualNovel
    {
        protected string name;
        protected string company; //change to [struct] Company company later
        protected string writer; //change to Author writer later
        protected bool coverArt; //change to Cover coverArt

        public VisualNovel(string name, string company, string writer, bool coverArt){
            this.name = name;
            this.company = company;
            this.writer = writer;
            this.coverArt = coverArt;
        }
    }
}
