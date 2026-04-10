using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Common.DTOs;

public class ProductDto
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ürün adı 2-100 karakter arasında olmalıdır.")]
    public string? Name { get; set; }
    
    [Required(ErrorMessage = "Kategori zorunludur.")]
    public string? Category { get; set; }
    
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<string>? ImageUrls { get; set; }

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0.1, 1000000, ErrorMessage = "Fiyat 0.1 ile 1.000.000 arasında olmalıdır.")]
    public decimal Price { get; set; }
    
    public List<string>? Features { get; set; }
    
    [Range(0, 10000, ErrorMessage = "Stok miktarı 0 ile 10.000 arasında olmalıdır.")]
    public int StockQuantity { get; set; }
    
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    
    public IFormFile? ImageFile { get; set; }
    public List<IFormFile>? AdditionalImageFiles { get; set; }
}
