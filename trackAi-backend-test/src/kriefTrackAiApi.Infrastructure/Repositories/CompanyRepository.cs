using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Infrastructure.Data;

namespace kriefTrackAiApi.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _context;

    public CompanyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Company>> GetAllAsync()
    {
        return await _context.Companies.ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(Guid id)
    {
        return await _context.Companies.FindAsync(id);
    }

    public async Task<Company> AddAsync(Company company)
    {
        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();
        return company;
    }

    public async Task<Company?> UpdateAsync(Guid id, Company company)
    {
        var existingCompany = await _context.Companies.FindAsync(id);
        if (existingCompany == null) return null;

        existingCompany.CustomerName = company.CustomerName;
        existingCompany.CustomerNumber = company.CustomerNumber;
        existingCompany.TAXNumber = company.TAXNumber;
        existingCompany.IsActive = company.IsActive;

        await _context.SaveChangesAsync();
        return existingCompany;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return false;

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<List<CustomerDto>> GetActiveCustomersByIdsAsync(List<Guid> companyIds)
    {
        return await _context.Companies
            .Where(c => companyIds.Contains(c.Id) && c.IsActive)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerNumber = c.CustomerNumber,
                CustomerName = c.CustomerName
            })
            .ToListAsync();
    }
    public async Task<Company?> GetByCustomerNumberAsync(int customerNumber)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.CustomerNumber == customerNumber);
    }
    public async Task<List<Company>> GetCompaniesByIdsAsync(List<Guid> companyIds)
    {
        return await _context.Companies
            .Where(c => companyIds.Contains(c.Id))
            .ToListAsync();
    }

}
