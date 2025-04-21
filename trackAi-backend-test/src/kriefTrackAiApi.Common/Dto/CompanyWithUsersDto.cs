namespace kriefTrackAiApi.Common.Dto
{
    public class CompanyWithUsersDto
    {
        public Guid Id { get; set; }
        public int CustomerNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int TAXNumber { get; set; }
        public bool IsActive { get; set; }

        public List<UserDto> Users { get; set; } = new();
    }
}
