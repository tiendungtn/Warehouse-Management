using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.DTOs;
using QuanLyKho.Models;

namespace QuanLyKho.Services;

public sealed class CategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.CategoryName)
            .Select(x => new CategoryDto(
                x.Id,
                x.CategoryName,
                x.Description,
                x.Products.Count))
            .ToListAsync();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CategoryDto(
                x.Id,
                x.CategoryName,
                x.Description,
                x.Products.Count))
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException(
                "Không tìm thấy danh mục.");
    }

    public async Task<CategoryDto> CreateAsync(
        CategoryRequest request)
    {
        var name = request.CategoryName.Trim();

        if (await _context.Categories.AnyAsync(
                x => x.CategoryName == name))
        {
            throw new InvalidOperationException(
                "Danh mục đã tồn tại.");
        }

        var entity = new Category
        {
            CategoryName = name,
            Description =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim()
        };

        _context.Categories.Add(entity);

        await _context.SaveChangesAsync();

        return new CategoryDto(
            entity.Id,
            entity.CategoryName,
            entity.Description,
            0);
    }

    public async Task<CategoryDto> UpdateAsync(
        int id,
        CategoryRequest request)
    {
        var entity = await _context.Categories
            .FindAsync(id)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy danh mục.");

        var name = request.CategoryName.Trim();

        if (await _context.Categories.AnyAsync(
                x => x.Id != id &&
                     x.CategoryName == name))
        {
            throw new InvalidOperationException(
                "Danh mục đã tồn tại.");
        }

        entity.CategoryName = name;

        entity.Description =
            string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Categories
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy danh mục.");

        if (entity.Products.Count > 0)
        {
            throw new InvalidOperationException(
                $"Không thể xóa danh mục {entity.CategoryName} " +
                "vì đang có sản phẩm liên kết.");
        }

        _context.Categories.Remove(entity);

        await _context.SaveChangesAsync();
    }
}