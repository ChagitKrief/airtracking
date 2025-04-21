using System;
using System.Collections.Generic;

namespace kriefTrackAiApi.Common.Dto
{
    public class CompanyDto
    {
    public Guid Id { get; set; } // Primary Key
    public int CustomerNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TAXNumber { get; set; }
    public bool IsActive { get; set; } = true;
    }
}
