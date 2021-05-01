using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Media
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Plot { get; set; }
        public string PosterPath { get; set; }
        public string VotesNumber { get; set; }
        public Director Director { get; set; }
        public string ReleaseDate { get; set; }
        public double Vote { get; set; }
        public string Url { get; set; }
        public int Year { get; set; }
        public List<Actor> Actors { get; set; } = new List<Actor>();
        public List<string> Genres { get; set; } = new List<string>();
        public List<string> MetaData { get; set; } = new List<string>();
    }
}