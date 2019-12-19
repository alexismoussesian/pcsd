using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcsd.plugin.sql.DataModel.Conversation
{
    public class Conversation
    {
        private IList<string> _requestedDivisionIds;

        [Key]
        public string conversationId { get; set; }
        public DateTime? conversationStart { get; set; }
        public DateTime? conversationEnd { get; set; }


        // public IList<string> divisionIds { get; set; } // commented as EF doesn't support list of string
        /// <summary>
        /// The code below is required because Entity Framework does not support collections of primitive data types (i.e. IList<string>)
        /// </summary>
        [NotMapped]
        public IList<string> divisionIds
        {
            get { return _requestedDivisionIds; }
            set
            {
                _requestedDivisionIds = value;
                authorizationDivision = new List<AuthorizationDivision>();
                foreach (var div in _requestedDivisionIds)
                {
                    authorizationDivision.Add(new AuthorizationDivision() { DivisionIds = div });
                }
            }
        }
        public IList<AuthorizationDivision> authorizationDivision { get; set; }

        public IList<Participant> participants { get; set; }        
        public bool IsFinished() => conversationEnd != null;
    }
}
