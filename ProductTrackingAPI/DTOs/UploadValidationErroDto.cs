namespace ProductTrackingAPI.DTOs
{
    public class UploadValidationErrorDto
    {
        public int RowNumber { get; set; }

        public string FieldName { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
