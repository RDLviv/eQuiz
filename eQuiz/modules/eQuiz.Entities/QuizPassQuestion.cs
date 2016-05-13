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
    
    public partial class QuizPassQuestion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public QuizPassQuestion()
        {
            this.UserAnswers = new HashSet<UserAnswer>();
        }
    
        public int Id { get; set; }
        public int QuizPassId { get; set; }
        public int QuestionId { get; set; }
        public int QuizBlockId { get; set; }
        public short QuestionOrder { get; set; }
    
        public virtual Question Question { get; set; }
        public virtual QuizBlock QuizBlock { get; set; }
        public virtual QuizPass QuizPass { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
    }
}
