﻿using FasterKv.Cache.Core.Abstractions;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.MessagePack;
using FasterKv.Cache.SystemTextJson;
using MessagePack;

namespace FasterKv.Cache.Core.Tests.KvStore;

public class FasterKvStoreTest : IDisposable
{
    private readonly FasterKvCache<Data> _fasterKv;

    private readonly Data _data = new()
    {
        One = "one",
        Two = 2
    };

    public FasterKvStoreTest()
    {
        _fasterKv = CreateKvStore();
    }

    private static FasterKvCache<Data> CreateKvStore()
    {
        return new FasterKvCache<Data>(null!,
            new DefaultSystemClock(),
            new FasterKvCacheOptions
            {
                SerializerName = "MessagePack",
                ExpiryKeyScanInterval = TimeSpan.Zero,
                IndexCount = 16384,
                MemorySizeBit = 10,
                PageSizeBit = 10,
                ReadCacheMemorySizeBit = 10,
                ReadCachePageSizeBit = 10,
                LogPath = "./unit-test/faster-kv-store-test"
            },
            new IFasterKvCacheSerializer[]
            {
                new MessagePackFasterKvCacheSerializer
                {
                    Name = "MessagePack"
                },
                new SystemTextJsonFasterKvCacheSerializer
                {
                    Name = "SystemTextJson"
                }
            },
            null);
    }

    [Fact]
    public void Set_Null_Value_Should_Get_Null_Value()
    {
        var guid = Guid.NewGuid().ToString("N");
        _fasterKv.Set(guid, null);

        var result = _fasterKv.Get(guid);
        Assert.Null(result);
    }

    [Fact]
    public void Set_Key_Should_Success()
    {
        var guid = Guid.NewGuid().ToString("N");
        _fasterKv.Set(guid, _data);

        var result = _fasterKv.Get(guid);

        Assert.Equal(_data, result);
    }
    
    [Fact]
    public void Set_Key_With_ExpiryTime_Should_Success()
    {
        var guid = Guid.NewGuid().ToString("N");
        _fasterKv.Set(guid, _data, TimeSpan.FromMinutes(1));

        var result = _fasterKv.Get(guid);
        
        Assert.Equal(_data, result);
    }
    

    [Fact]
    public void Get_Not_Exist_Key_Should_Return_Null()
    {
        var guid = Guid.NewGuid().ToString("N");
        var result = _fasterKv.Get(guid);
        Assert.Null(result);
    }

    [Fact]
    public void Delete_Key_Should_Success()
    {
        var guid = Guid.NewGuid().ToString("N");
        _fasterKv.Set(guid, _data);
        _fasterKv.Delete(guid);

        var result = _fasterKv.Get(guid);
        Assert.Null(result);
    }
    
    
    [Fact]
    public async Task SetAsync_Null_Value_Should_Get_Null_Value()
    {
        var guid = Guid.NewGuid().ToString("N");
        await _fasterKv.SetAsync(guid, null); 

        var result = await _fasterKv.GetAsync(guid);
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_Key_Should_Success()
    {
        var guid = Guid.NewGuid().ToString("N");
        await _fasterKv.SetAsync(guid, _data);

        var result = await _fasterKv.GetAsync(guid);

        Assert.Equal(_data, result);
    }
    
    [Fact]
    public async Task SetAsync_Key_With_ExpiryTime_Should_Success()
    {
        var guid = Guid.NewGuid().ToString("N");
        await _fasterKv.SetAsync(guid, _data, TimeSpan.FromMinutes(1));

        var result = await _fasterKv.GetAsync(guid);
        
        Assert.Equal(_data, result);
    }
    

    [Fact]
    public async Task GetAsync_Not_Exist_Key_Should_Return_Null()
    {
        var guid = Guid.NewGuid().ToString("N");
        var result = await _fasterKv.GetAsync(guid);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_Key_Should_Success()
    {
        var guid = Guid.NewGuid().ToString("N");
        await _fasterKv.SetAsync(guid, _data);
        await _fasterKv.DeleteAsync(guid);

        var result = await _fasterKv.GetAsync(guid);
        Assert.Null(result);
    }

    [Fact]
    public void Set_Big_DataSize_Should_Success()
    {
        int nums = 1000;
        for (int i = 0; i < nums; i++)
        {
            _fasterKv.Set($"big_data_{i}", new Data
            {
                One = i.ToString(),
                Two = i
            });
        }

        for (int i = 0; i < nums; i++)
        {
            var value = _fasterKv.Get($"big_data_{i}");
            Assert.NotNull(value);
            Assert.Equal(i.ToString(), value!.One);
            Assert.Equal(i, value.Two);
        }
    }

    [Fact]
    public async Task SetAsync_Big_DataSize_Should_Success()
    {
        int nums = 1000;
        for (int i = 0; i < nums; i++)
        {
            await _fasterKv.SetAsync($"big_data_{i}", new Data
            {
                One = i.ToString(),
                Two = i
            });
        }

        for (int i = 0; i < nums; i++)
        {
            var value = await _fasterKv.GetAsync($"big_data_{i}");
            Assert.NotNull(value);
            Assert.Equal(i.ToString(), value!.One);
            Assert.Equal(i, value.Two);
        }
    }
    
    [Fact]
    public void Set_Big_DataSize_With_ExpiryTime_Should_Success()
    {
        int nums = 1000;
        for (int i = 0; i < nums; i++)
        {
            _fasterKv.Set($"big_data_{i}", new Data
            {
                One = i.ToString(),
                Two = i
            }, TimeSpan.FromMinutes(5));
        }

        for (int i = 0; i < nums; i++)
        {
            var value = _fasterKv.Get($"big_data_{i}");
            Assert.NotNull(value);
            Assert.Equal(i.ToString(), value!.One);
            Assert.Equal(i, value.Two);
        }
    }

    [Fact]
    public void Set_Big_DataSize_And_Repeat_Reading_Should_Success()
    {
        int nums = 1000;
        for (int i = 0; i < nums; i++)
        {
            _fasterKv.Set($"big_value_{i}", new Data
            {
                One = i.ToString(),
                Two = i
            });
        }
        
        var value = _fasterKv.Get($"big_value_{0}");
        Assert.NotNull(value);
        Assert.Equal(0.ToString(), value!.One);
        Assert.Equal(0, value.Two);


        value = _fasterKv.Get($"big_value_{0}");
        Assert.NotNull(value);
        Assert.Equal(0.ToString(), value!.One);
        Assert.Equal(0, value.Two);
    }

    [Fact]
    public void Set_Big_Value_Should_Success()
    {
        // 4MB value
        var bigValues = Enumerable.Range(0, 4 * 1024 * 1024).Select(i => (byte) i).ToArray();
        
        int nums = 200;
        for (int i = 0; i < nums; i++)
        {
            _fasterKv.Set($"big_value_{i}", new Data
            {
                One = i.ToString(),
                Two = i,
                Three = bigValues
            });
        }

        for (int i = 0; i < nums; i++)
        {
            var result = _fasterKv.Get($"big_value_{i}");
        
            Assert.NotNull(result?.Three);
            Assert.Equal(i.ToString(), result!.One);
            Assert.Equal(i, result.Two);
            Assert.True(bigValues.SequenceEqual(result.Three!));   
        }
    }
    
        
    [Fact]
    public void Set_Big_DataSize_With_Expired_Should_Return_Null()
    {
        int nums = 1000;
        for (int i = 0; i < nums; i++)
        {
            _fasterKv.Set($"Set_Big_DataSize_With_Expired_Should_Return_Null_{i}", new Data
            {
                One = i.ToString(),
                Two = i
            }, TimeSpan.FromSeconds(1));
        }
        
        Thread.Sleep(1000);

        for (int i = 0; i < nums; i++)
        {
            var value = _fasterKv.Get($"Set_Big_DataSize_With_Expired_Should_Return_Null_{i}");
            Assert.Null(value);
        }
    }
    
    [Fact]
    public void Set_Big_DataSize_With_Random_Expired_Should_Success()
    {
        int nums = 1000;
        for (int i = 0; i < nums; i++)
        {
            _fasterKv.Set($"Set_Big_DataSize_With_Random_Expired_Should_Success_{i}", new Data
            {
                One = i.ToString(),
                Two = i
            }, i % 2 == 0 ? TimeSpan.FromSeconds(1) : TimeSpan.FromMinutes(1));
        }
        
        Thread.Sleep(1000);

        for (int i = 0; i < nums; i++)
        {
            var value = _fasterKv.Get($"Set_Big_DataSize_With_Random_Expired_Should_Success_{i}");
            if (i % 2 == 0)
            {
                Assert.Null(value);   
            }
            else
            {
                Assert.NotNull(value);
                Assert.NotNull(value);
                Assert.Equal(i.ToString(), value!.One);
                Assert.Equal(i, value.Two);
            }
        }
    }

    public void Dispose()
    {
        _fasterKv.Dispose();
    }
}

[MessagePackObject]
public class Data
{
    [Key(0)] public string? One { get; set; }

    [Key(1)] public long Two { get; set; }
    
    [Key(2)] public byte[]? Three { get; set; }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    protected bool Equals(Data other)
    {
        return One == other.One && Two == other.Two;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(One, Two);
    }
}