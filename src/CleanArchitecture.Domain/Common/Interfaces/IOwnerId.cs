using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Domain.Common.Interfaces;

public interface IOwnerId
{
    [MaxLength(LengthConstants.UserIdMaxLength)]
    public string UserId { get; set; }
}
