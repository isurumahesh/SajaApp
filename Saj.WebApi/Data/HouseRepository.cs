using Microsoft.EntityFrameworkCore;

public interface IHouseRepository
{
    public Task<List<HouseEntity>> GetAll();
    public Task<HouseEntity> Get(int id);
     Task<HouseDetailDto> Add(HouseDetailDto house);
    Task<HouseDetailDto> Update(HouseDetailDto house);
    Task Delete(int id);
}

public class HouseRepository : IHouseRepository
{

    private readonly HouseDbContext context;
    public HouseRepository(HouseDbContext context)
    {
        this.context = context;
    }

    public async Task<List<HouseEntity>> GetAll()
    {
        return await context.Houses.ToListAsync();
    }

    public async Task<HouseEntity> Get(int id)
    {
        return await context.Houses.FirstOrDefaultAsync(a => a.Id == id);
    }

     public async Task<HouseDetailDto> Add(HouseDetailDto dto)
    {
        var entity = new HouseEntity();
        DtoToEntity(dto, entity);
        context.Houses.Add(entity);
        await context.SaveChangesAsync();
        return EntityToDetailDto(entity);
    }

    public async Task<HouseDetailDto> Update(HouseDetailDto dto)
    {
        var entity = await context.Houses.FindAsync(dto.Id);
        if (entity == null)
            throw new ArgumentException($"Trying to update house: entity with ID {dto.Id} not found.");
        DtoToEntity(dto, entity);
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return EntityToDetailDto(entity);
    }

    public async Task Delete(int id)
    {
        var entity = await context.Houses.FindAsync(id);
        if (entity == null)
            throw new ArgumentException($"Trying to delete house: entity with ID {id} not found.");
        context.Houses.Remove(entity);
        await context.SaveChangesAsync();
    }

    
    private static void DtoToEntity(HouseDetailDto dto, HouseEntity e)
    {
        e.Address = dto.Address;
        e.Country = dto.Country;
        e.Description = dto.Description;
        e.Price = dto.Price;
        e.Photo = dto.Photo;
    }

     private static HouseDetailDto EntityToDetailDto(HouseEntity e)
    {
        return new HouseDetailDto(e.Id, e.Address, e.Country, e.Description, e.Price, e.Photo);
    }
}