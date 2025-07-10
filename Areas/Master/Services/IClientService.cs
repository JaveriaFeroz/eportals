using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IClientService
    {
        Task<IEnumerable<ClientListViewModel>> GetClientsAsync(bool activeOnly = true, string searchTerm = null);
        Task<IEnumerable<ClientRWBViewModel>> GetClientsForRWBAsync(bool activeOnly = true);
        Task<ClientViewModel> GetClientByIdAsync(short id);
        Task<bool> SaveClientAsync(ClientViewModel model);
        Task<bool> DeleteClientAsync(short id);
        Task<bool> ClientNameExistsAsync(string clientName, short? excludeId = null);
        Task<ClientLookupsViewModel> GetLookupsAsync();
      //  Task<IEnumerable<WorkflowClientViewModel>> GetPendingWorkflowFormsAsync();
    }

    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientService> _logger;

        public ClientService(ApplicationDbContext context, ILogger<ClientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ClientListViewModel>> GetClientsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Clients
                    .Include(x => x.City)
                    .Include(x => x.IndustryVertical)
                    .Include(x => x.PaymentMode)
                    .Include(x => x.RateType)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.ClientName.Contains(searchTerm) ||
                                           x.ShortName.Contains(searchTerm) ||
                                           x.ContactPerson.Contains(searchTerm) ||
                                           x.Email.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.ClientName)
                    .Select(x => new ClientListViewModel
                    {
                        ClientId = x.ClientId,
                        ClientName = x.ClientName,
                        ShortName = x.ShortName,
                        CityName = x.City != null ? x.City.CityName : "",
                        ContactPerson = x.ContactPerson,
                        ContactNo = x.ContactNo,
                        Email = x.Email,
                        CategoryMandatory = x.CategoryMandatory,
                        ProductMandatory = x.ProductMandatory,
                        DetGraceHRs = x.DetGraceHRs,
                        RateTypeName = x.RateType != null ? x.RateType.RateTypeName : "",
                        PaymentModeName = x.PaymentMode != null ? x.PaymentMode.PaymentModeName : "",
                        TaxRate = x.TaxRate,
                        IsActive = x.IsActive,
                        CreatedBy = x.CreatedByUser != null ? $"{x.CreatedByUser.FirstName} {x.CreatedByUser.LastName}" : "System",
                        CreatedOn = x.CreatedOn,
                        UpdatedBy = x.UpdatedByUser != null ? $"{x.UpdatedByUser.FirstName} {x.UpdatedByUser.LastName}" : null,
                        UpdatedOn = x.UpdatedOn
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients");
                throw;
            }
        }

        public async Task<IEnumerable<ClientRWBViewModel>> GetClientsForRWBAsync(bool activeOnly = true)
        {
            try
            {
                var query = _context.Clients.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                var result = await query
                    .OrderBy(x => x.ClientName)
                    .Select(x => new ClientRWBViewModel
                    {
                        ClientId = x.ClientId,
                        ClientName = x.ClientName,
                        CategoryMandatory = x.CategoryMandatory,
                        ProductMandatory = x.ProductMandatory,
                        DetGraceHRs = x.DetGraceHRs,
                        RateTypeId = x.RateTypeId,
                        PaymentModeId = x.PaymentModeId ?? 0
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients for RWB");
                throw;
            }
        }

        public async Task<ClientViewModel> GetClientByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Clients
                    .Include(x => x.City)
                    .Include(x => x.IndustryVertical)
                    .Include(x => x.PaymentMode)
                    .Include(x => x.RateType)
                    .Include(x => x.ClientInvoiceFormats)
                        .ThenInclude(cif => cif.InvoiceFormat)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.ClientId == id);

                if (entity == null) return null;

                return new ClientViewModel
                {
                    ClientId = entity.ClientId,
                    AccountId = entity.AccountId,
                    ClientName = entity.ClientName,
                    ShortName = entity.ShortName,
                    Address = entity.Address,
                    CompanyId = entity.CompanyId,
                    CityId = entity.CityId,
                    IndustryVerticalId = entity.IndustryVerticalId,
                    ContractPeriod = entity.ContractPeriod,
                    ContactNo = entity.ContactNo,
                    Email = entity.Email,
                    URL = entity.URL,
                    ContactPerson = entity.ContactPerson,
                    PaymentModeId = entity.PaymentModeId,
                    CreditLimit = entity.CreditLimit,
                    CreditDays = entity.CreditDays,
                    RateTypeId = entity.RateTypeId,
                    CWClientId = entity.CWClientId,
                    NTN = entity.NTN,
                    STRN = entity.STRN,
                    IsActive = entity.IsActive,
                    CategoryMandatory = entity.CategoryMandatory,
                    ProductMandatory = entity.ProductMandatory,
                    DetGraceHRs = entity.DetGraceHRs,
                    TaxRate = entity.TaxRate,
                    CompanyName = entity.Company?.CompanyName,
                    CityName = entity.City?.CityName,
                    IndustryVerticalName = entity.IndustryVertical?.IndustryVerticalName,
                    PaymentModeName = entity.PaymentMode?.PaymentModeName,
                    RateTypeName = entity.RateType?.RateTypeName,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn,
                    InvoiceFormats = entity.ClientInvoiceFormats.Select(cif => new ClientInvoiceFormatViewModel
                    {
                        DetailId = cif.DetailId,
                        FormatId = cif.FormatId,
                        FormatName = cif.InvoiceFormat?.FormatName,
                        IsSelected = true
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client with ID {ClientId}", id);
                throw;
            }
        }

        public async Task<bool> SaveClientAsync(ClientViewModel model)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        Client entity;

        // Check if we are UPDATING an existing client
        if (model.ClientId.HasValue && model.ClientId > 0)
        {
            entity = await _context.Clients
                .Include(x => x.ClientInvoiceFormats)
                .FirstOrDefaultAsync(x => x.ClientId == model.ClientId.Value);

            if (entity == null)
            {
                _logger.LogWarning("Attempted to update a client that does not exist. ID: {ClientId}", model.ClientId);
                return false;
            }

            // --- UPDATE LOGIC ---
            // Map ALL properties from ViewModel to the database Entity
            entity.AccountId = model.AccountId;
            entity.ClientName = model.ClientName;
            entity.ShortName = model.ShortName;
            entity.Address = model.Address;
            entity.CompanyId = model.CompanyId;
            entity.CityId = model.CityId;
            entity.IndustryVerticalId = model.IndustryVerticalId;
            entity.ContractPeriod = model.ContractPeriod;
            entity.ContactNo = model.ContactNo;
            entity.Email = model.Email;
            entity.URL = model.URL;
            entity.ContactPerson = model.ContactPerson;
            entity.PaymentModeId = model.PaymentModeId;
            entity.CreditLimit = model.CreditLimit.GetValueOrDefault(); // FIX: Handle nullable
            entity.CreditDays = model.CreditDays;
            entity.RateTypeId = model.RateTypeId;
            entity.CWClientId = model.CWClientId;
            entity.NTN = model.NTN;
            entity.STRN = model.STRN;
            entity.IsActive = model.IsActive;
            entity.CategoryMandatory = model.CategoryMandatory;
            entity.ProductMandatory = model.ProductMandatory;
            entity.DetGraceHRs = model.DetGraceHRs.GetValueOrDefault(); // FIX: Handle nullable
            entity.TaxRate = model.TaxRate.GetValueOrDefault();       // FIX: Handle nullable
            
        }
        else // Otherwise, we are CREATING a new client
        {
            // --- CREATE LOGIC ---
            entity = new Client
            {
                AccountId = model.AccountId,
                ClientName = model.ClientName,
                ShortName = model.ShortName,
                Address = model.Address,
                CompanyId = model.CompanyId,
                CityId = model.CityId,
                IndustryVerticalId = model.IndustryVerticalId,
                ContractPeriod = model.ContractPeriod,
                ContactNo = model.ContactNo,
                Email = model.Email,
                URL = model.URL,
                ContactPerson = model.ContactPerson,
                PaymentModeId = model.PaymentModeId,
                CreditLimit = model.CreditLimit.GetValueOrDefault(), // FIX: Handle nullable
                CreditDays = model.CreditDays,
                RateTypeId = model.RateTypeId,
                CWClientId = model.CWClientId,
                NTN = model.NTN,
                STRN = model.STRN,
                IsActive = model.IsActive,
                CategoryMandatory = model.CategoryMandatory,
                ProductMandatory = model.ProductMandatory,
                DetGraceHRs = model.DetGraceHRs.GetValueOrDefault(), // FIX: Handle nullable
                TaxRate = model.TaxRate.GetValueOrDefault(),       // FIX: Handle nullable


            };
            _context.Clients.Add(entity);
        }

        // Save changes to get the ClientId if it's new
        await _context.SaveChangesAsync();

        // Handle the related invoice formats after the main client is saved
        if (model.InvoiceFormats != null)
        {
            await SaveClientInvoiceFormatsAsync(entity.ClientId, model.InvoiceFormats);
        }

        // Final save for invoice formats
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
        return true;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error saving client");
        throw;
    }
}

        private async Task SaveClientInvoiceFormatsAsync(short clientId, List<ClientInvoiceFormatViewModel> formats)
        {
            // Get existing formats
            var existingFormats = await _context.ClientInvoiceFormats
                .Where(x => x.ClientId == clientId)
                .ToListAsync();

            // Remove unselected formats
            var formatsToRemove = existingFormats
                .Where(ef => !formats.Any(f => f.FormatId == ef.FormatId && f.IsSelected))
                .ToList();

            _context.ClientInvoiceFormats.RemoveRange(formatsToRemove);

            // Add new selected formats
            var newFormats = formats
                .Where(f => f.IsSelected && !existingFormats.Any(ef => ef.FormatId == f.FormatId))
                .Select(f => new ClientInvoiceFormat
                {
                    ClientId = clientId,
                    FormatId = f.FormatId.Value
                })
                .ToList();

            _context.ClientInvoiceFormats.AddRange(newFormats);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteClientAsync(short id)
        {
            try
            {
                var entity = await _context.Clients.FindAsync(id);
                if (entity == null) return false;

                _context.Clients.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client with ID {ClientId}", id);
                throw;
            }
        }

        public async Task<bool> ClientNameExistsAsync(string clientName, short? excludeId = null)
        {
            try
            {
                var query = _context.Clients.Where(x => x.ClientName == clientName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ClientId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if client name exists");
                throw;
            }
        }

        public async Task<ClientLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new ClientLookupsViewModel();

                lookups.Cities = await _context.Cities
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.CityName)
                    .Select(x => new CityLookupViewModel
                    {
                        CityId = x.CityId,
                        CityName = x.CityName
                    })
                    .ToListAsync();

                lookups.Companies = await _context.Companies
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.CompanyName)
                   .Select(x => new CompanyLookupViewModel
                   {
                       CompanyId = x.CompanyId,
                       CompanyName = x.CompanyName
                   })
                   .ToListAsync();

                lookups.PaymentModes = await _context.PaymentModes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.PaymentModeName)
                    .Select(x => new PaymentModeLookupViewModel
                    {
                        PaymentModeId = x.PaymentModeId,
                        PaymentModeName = x.PaymentModeName
                    })
                    .ToListAsync();

                lookups.IndustryVerticals = await _context.IndustryVerticals
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.IndustryVerticalName)
                    .Select(x => new IndustryVerticalLookupViewModel
                    {
                        IndustryVerticalId = x.IndustryVerticalId,
                        IndustryName = x.IndustryVerticalName
                    })
                    .ToListAsync();

                lookups.InvoiceFormats = await _context.InvoiceFormats
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.FormatName)
                    .Select(x => new InvoiceFormatLookupViewModel
                    {
                        FormatId = x.FormatId,
                        FormatName = x.FormatName
                    })
                    .ToListAsync();

                lookups.RateTypes = await _context.RateTypes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.RateTypeName)
                    .Select(x => new RateTypeLookupViewModel
                    {
                        RateTypeId = x.RateTypeId,
                        RateTypeName = x.RateTypeName
                    })
                    .ToListAsync();

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                throw;
            }
        }

        //public async Task<IEnumerable<WorkflowClientViewModel>> GetPendingWorkflowFormsAsync()
        //{
        //    try
        //    {
        //        // This would depend on your workflow implementation
        //        // Assuming you have a WorkflowForms table or similar
        //        var result = await _context.WorkflowForms
        //            .Include(x => x.Client)
        //            .Include(x => x.WorkflowState)
        //            .Where(x => x.IsActive && !x.IsCompleted)
        //            .Select(x => new WorkflowClientViewModel
        //            {
        //                FormId = x.FormId,
        //                ClientId = x.ClientId,
        //                ClientName = x.Client.ClientName,
        //                StateName = x.WorkflowState.StateName
        //            })
        //            .ToListAsync();

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving pending workflow forms");
        //        throw;
        //    }
        //}
    }
}