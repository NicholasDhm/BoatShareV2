using boat_share.Abstract;
using boat_share.Data;
using boat_share.DTOs;
using boat_share.Models;
using Microsoft.EntityFrameworkCore;

namespace boat_share.Services
{
    public class BoatService : IBoatService
    {
        private readonly ApplicationDbContext _context;

        public BoatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BoatDTO>> GetBoatsAsync()
        {
            return await _context.Boats
                .Select(b => new BoatDTO
                {
                    BoatId = b.BoatId,
                    Name = b.Name,
                    Type = b.Type,
                    Description = b.Description,
                    Location = b.Location,
                    Capacity = b.Capacity,
                    HourlyRate = b.HourlyRate,
                    IsActive = b.IsActive,
                    AssignedUsersCount = b.AssignedUsersCount,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<BoatDTO?> GetBoatByIdAsync(int boatId)
        {
            var boat = await _context.Boats.FindAsync(boatId);
            if (boat == null) return null;

            return new BoatDTO
            {
                BoatId = boat.BoatId,
                Name = boat.Name,
                Type = boat.Type,
                Description = boat.Description,
                Location = boat.Location,
                Capacity = boat.Capacity,
                HourlyRate = boat.HourlyRate,
                IsActive = boat.IsActive,
                AssignedUsersCount = boat.AssignedUsersCount,
                CreatedAt = boat.CreatedAt,
                UpdatedAt = boat.UpdatedAt
            };
        }

        public async Task<Boat> CreateBoatAsync(BoatCreateDTO boatCreateDto)
        {
            var boat = new Boat
            {
                Name = boatCreateDto.Name,
                Type = boatCreateDto.Type,
                Description = boatCreateDto.Description,
                Location = boatCreateDto.Location,
                Capacity = boatCreateDto.Capacity,
                HourlyRate = boatCreateDto.HourlyRate
            };

            _context.Boats.Add(boat);
            await _context.SaveChangesAsync();

            return boat;
        }

        public async Task<BoatDTO?> UpdateBoatAsync(int boatId, BoatUpdateDTO boatUpdateDto)
        {
            var boat = await _context.Boats.FindAsync(boatId);
            if (boat == null) return null;

            if (!string.IsNullOrEmpty(boatUpdateDto.Name))
                boat.Name = boatUpdateDto.Name;

            if (!string.IsNullOrEmpty(boatUpdateDto.Type))
                boat.Type = boatUpdateDto.Type;

            if (!string.IsNullOrEmpty(boatUpdateDto.Description))
                boat.Description = boatUpdateDto.Description;

            if (!string.IsNullOrEmpty(boatUpdateDto.Location))
                boat.Location = boatUpdateDto.Location;

            if (boatUpdateDto.Capacity.HasValue)
                boat.Capacity = boatUpdateDto.Capacity.Value;

            if (boatUpdateDto.HourlyRate.HasValue)
                boat.HourlyRate = boatUpdateDto.HourlyRate.Value;

            if (boatUpdateDto.IsActive.HasValue)
                boat.IsActive = boatUpdateDto.IsActive.Value;

            boat.MarkAsUpdated();
            await _context.SaveChangesAsync();

            return await GetBoatByIdAsync(boatId);
        }

        public async Task<bool> DeleteBoatAsync(int boatId)
        {
            var boat = await _context.Boats.FindAsync(boatId);
            if (boat == null) return false;

            _context.Boats.Remove(boat);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignUserToBoatAsync(int boatId, int userId)
        {
            var boat = await _context.Boats.FindAsync(boatId);
            var user = await _context.Users.FindAsync(userId);

            if (boat == null || user == null) return false;

            user.BoatId = boatId;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
