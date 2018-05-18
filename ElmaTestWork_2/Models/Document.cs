using AbstractApplication.Domain;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElmaTestWork_2.Models
{
    public class Document : EntityWithTypedId<string>, IValidatableObject
    {
        public virtual string OriginalName { get; set; }

        [Display(Name = "Название документа")]
        [Required(ErrorMessage = "Укажите название документа.")]
        [MaxLength(300, ErrorMessage = "Максимальная длина не должна превышать 300 символов.")]
        public virtual string Description { get; set; }

        [Display(Name = "Дата документа")]
        public virtual DateTime Date { get; set; }

        [Display(Name = "Автор")]
        public virtual string Author { get; set; }

        public virtual string FileName { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Description.Trim().Length == 0)
                yield return new ValidationResult("Название документа не может быть пустым.");
            if (Date == null)
                Date = DateTime.Now.Date;

        }
    }

    public class DocumentMap : ClassMap<Document>
    {
        public DocumentMap()
        {
            Id(x => x.Id).GeneratedBy.UuidHex("D");
            Map(x => x.OriginalName);
            Map(x => x.Description);
            Map(x => x.Date);
            Map(x => x.Author);
            Map(x => x.FileName);
            //Map(x => x.Content);
            SqlInsert("EXEC sp_Document_Create @OriginalName=?, @Description=?, @Date=?, @Author=?, @FileName=?, @Id=?");
        }
    }
}