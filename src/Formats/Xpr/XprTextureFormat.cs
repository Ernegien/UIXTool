namespace UIXTool.Formats.Xpr
{
    /// <summary>
    /// Maps 1:1 with NV097_SET_TEXTURE_FORMAT_COLOR types.
    /// </summary>
    public enum XprTextureFormat
    {
        SZ_Y8 = 0x0,
        SZ_A1R5G5B5 = 0x02,
        SZ_X1R5G5B5 = 0x03,
        SZ_A4R4G4B4 = 0x04,
        SZ_R5G6B5 = 0x05,
        SZ_A8R8G8B8 = 0x06,
        SZ_X8R8G8B8 = 0x07,
        SZ_I8_A8R8G8B8 = 0x0B,
        L_DXT1_A1R5G5B5 = 0x0C,
        L_DXT23_A8R8G8B8 = 0x0E,
        L_DXT45_A8R8G8B8 = 0x0F,
        LU_IMAGE_R5G6B5 = 0x11,
        LU_IMAGE_A8R8G8B8 = 0x12,
        SZ_A8 = 0x19,
        LU_IMAGE_X8R8G8B8 = 0x1E,
        LU_IMAGE_DEPTH_Y16_FIXED = 0x30,
        SZ_A8B8G8R8 = 0x3A,
        SZ_R8G8B8A8 = 0x3C,
        LU_IMAGE_A8B8G8R8 = 0x3F
    }
}
