using AutoMapper;
using LMS.Server.Core.Domain;
using LMS.Server.Core.Dto;
using LMS.Server.Core.Interfaces;
using LMS.Server.Core.Specifications;
using LMS.Server.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Server.Infrastructure.Repositories
{
    public sealed class CourseRepository : EfRepository<TCourse>, ICourseRepository
    {
        private readonly IMapper _mapper;

        public CourseRepository(IMapper mapper, AppDbContext appDbContext) : base(appDbContext)
        {
            _mapper = mapper;
        }

        public async Task<CourseDto[]> ListPagination(int pageSize, int pageIndex)
        {
            List<TCourse> courses = await ListBySpec(new CoursePaginatedSpecification(skip: pageIndex * pageSize, take: pageSize));
            CourseDto[] response = courses.Select(_mapper.Map<CourseDto>).ToArray();
            return response;
        }
    }
}