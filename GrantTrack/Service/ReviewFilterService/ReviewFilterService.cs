using System;
using System.Text.Json;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto;
using GrantTrack.Repository.ReviewRepository;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.ReviewFilterService;

public class ReviewFilterService : IReviewFilterService
{
    private readonly IReviewRepository _reviewRepo;

    public ReviewFilterService(IReviewRepository reviewRepo)
    {
        _reviewRepo = reviewRepo;
    }

    public async Task<List<ReviewFilterResponseDto>> GetPagedReviewsAsync(ReviewFilterRequestDto filter)
    {
        return await _reviewRepo.GetFilteredReviewsAsync(filter);
    }
}