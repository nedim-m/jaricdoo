﻿using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace backend.Services
{
    public class ImageServices : IImageServices
    {
        private readonly DataContext _context;

        public ImageServices(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> AddImages(int projectId, List<string> images)
        {
            var project = await _context.Projects.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                throw new KeyNotFoundException($"Projekat sa ID-om {projectId} nije pronađen.");
            }

            foreach (var img in images)
            {
                if (string.IsNullOrWhiteSpace(img))
                {
                    throw new ArgumentException("Jedna ili više slika su prazne ili nevalidne.");
                }

                try
                {
                    var mimeType = img.Substring(5, img.IndexOf(";") - 5);
                    var base64Data = img.Substring(img.IndexOf(",") + 1);
                    byte[] imageBytes = Convert.FromBase64String(base64Data);


                    using (var image = SixLabors.ImageSharp.Image.Load(imageBytes))
                    {

                        image.Mutate(x => x.Resize(1920, 1024));


                        using (var ms = new MemoryStream())
                        {
                            image.SaveAsJpeg(ms);
                            var resizedImageBytes = ms.ToArray();

                            var imageEntity = new Data.Image
                            {
                                Img = resizedImageBytes,
                                MimeType = mimeType,
                                Project = project
                            };

                            _context.Images.Add(imageEntity);
                        }
                    }
                }
                catch (FormatException ex)
                {
                    throw new Exception($"Slika nije validan Base64 format: {img}", ex);
                }
            }

            await _context.SaveChangesAsync();
            return true; // Vraća true ako su slike uspešno dodate
        }

        public async Task<bool> DeleteImages(int projectId, List<int> imageIds)
        {
            var project = await _context.Projects.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                throw new KeyNotFoundException($"Projekat sa ID-om {projectId} nije pronađen.");
            }

            var imagesToDelete = project.Images.Where(i => imageIds.Contains(i.Id)).ToList();
            if (imagesToDelete.Count == 0)
            {
                throw new Exception("Nema slika za brisanje.");
            }

            _context.Images.RemoveRange(imagesToDelete);

            try
            {
                await _context.SaveChangesAsync();
                return true; // Vraća true ako su slike uspešno obrisane
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška prilikom brisanja slika: {ex.Message}", ex);
            }
        }

        public async Task<List<ImageResponse>> GetImagesByProjectId(int projectId)
        {
            var images = await _context.Images
                .Where(img => img.ProjectId == projectId)
                .Select(img => new ImageResponse
                {
                    Id = img.Id,
                    MimeType = img.MimeType,
                    Img = $"data:{img.MimeType};base64,{Convert.ToBase64String(img.Img)}"
                })
                .ToListAsync();

            return images;
        }

        public async Task<bool> SetAsCover(int projectId, int imageId)
        {
            var images = await _context.Images
                .Where(i => i.ProjectId == projectId)
                .ToListAsync();

            if (!images.Any())
                throw new Exception("Nema slika za navedeni projekat.");

            // Resetovanje Cover statusa na false za sve slike
            foreach (var image in images)
            {
                image.Cover = false;
            }

            // Pronađi sliku koja treba da postane naslovna
            var coverImage = images.FirstOrDefault(i => i.Id == imageId);

            if (coverImage == null)
                throw new Exception("Slika sa datim ID-jem nije pronađena.");

            // Postavljanje Cover na true
            coverImage.Cover = true;

            // Snimanje promena u bazu podataka
            try
            {
                await _context.SaveChangesAsync();
                return true; // Vraćanje true ako je uspešno sačuvano
            }
            catch (Exception)
            {
                return false; // Ako dođe do greške prilikom snimanja, vraća false
            }
        }


    }
}