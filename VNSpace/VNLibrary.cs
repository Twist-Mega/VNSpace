using System;
using System.Collections.Generic;
using System.Text;

namespace VNSpace
{
    internal class VNLibrary
    {

        private List<UserEntry> vnList; //list of vns in library
        private List<string> categories;

        //constructor
        public VNLibrary()
        {
           this.vnList = new List<UserEntry>();
           this.categories = new List<string>();
        }

        //add VNDB-linked visual novel
        public void AddVN(string name, string company, string writer, string? coverArt, string? id)
        {
            VisualNovel vn = new VisualNovel(name, company, writer, coverArt, id);
            UserEntry entry = new UserEntry(vn, statusLabel.Planning);
            vnList.Add(entry);
        }

        //add non-VNDB-linked visual novel
        public void AddVN(string name, string company, string writer)
        {
            VisualNovel vn = new VisualNovel(name, company, writer);
            UserEntry entry = new UserEntry(vn, statusLabel.Planning);
            vnList.Add(entry);
        }

        //remove visual novel from library
        public void RemoveVN(UserEntry entry)
        {
            vnList.Remove(entry);
        }

        //get all vn entries
        public List<UserEntry> GetAllEntries()
        {
            return vnList;
        }

        //get vn entries filtered by category
        public List<UserEntry> GetAllEntriesbyCategory(string category)
        {
            List<UserEntry> filteredList = new List<UserEntry>();
            foreach(UserEntry entry in vnList)
            {
                if (entry.GetCategory() == category) //add vns in given cateogry to filtered list
                {
                    filteredList.Add(entry);
                }
            }

            return filteredList;
        }

        //get vn entries filtered by status
        public List<UserEntry> GetAllEntriesbyStatus(statusLabel status)
        {
            List<UserEntry> filteredList = new List<UserEntry>();
            foreach (UserEntry entry in vnList)
            {
                if (entry.GetStatus() == status) //add vns with given status to filtered list
                {
                    filteredList.Add(entry);
                }
            }

            return filteredList;
        }


    }
}
