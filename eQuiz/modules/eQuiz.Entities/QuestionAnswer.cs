//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eQuiz.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class QuestionAnswer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public QuestionAnswer()
        {
            this.tblUserAnswers = new HashSet<UserAnswer>();
        }
    
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; }
        public Nullable<byte> AnswerOrder { get; set; }
        public Nullable<bool> IsRight { get; set; }
    
        public virtual Question tblQuestion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserAnswer> tblUserAnswers { get; set; }
    }
}
