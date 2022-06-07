using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Gateways
{
    public class UserGateway : IUserGateway
    {
        private readonly BrokerageContext _context;

        public UserGateway(BrokerageContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync(UserRole? role = null)
        {
            if (role == null)
            {
                return await _context.Users
                    .Where(u => u.IsActive == true)
                    .OrderBy(u => u.Name)
                    .ToListAsync();
            }
            else
            {
                return await _context.Users
                    .Where(u => u.IsActive == true)
                    .Where(u => u.Roles.Contains((UserRole) role))
                    .OrderBy(u => u.Name)
                    .ToListAsync();
            }
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => u.IsActive == true)
                .Where(u => u.Email == email)
                .SingleOrDefaultAsync();
        }

        public async Task<User> CreateUser(string email, string name)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new InvalidOperationException($"User with email address {email} already exists");
            }

            var user = new User
            {
                Email = email,
                Name = name,
                IsActive = true
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
