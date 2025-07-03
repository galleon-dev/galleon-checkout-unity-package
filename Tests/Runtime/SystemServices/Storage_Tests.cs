using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Storage_Tests
{
    private Storage storage;
        
    [SetUp]
    public void SetUp()
    {
        storage = new Storage();
        
        // Clear any existing data before each test
        storage.ClearAll();
    }
        
    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        storage.ClearAll();
    }
        
    [Test]
    public void Write_WithCustomKey_StoresValueCorrectly()
    {
        // Arrange
        string testKey   = "test_key";
        string testValue = "test_value";
            
        // Act
        storage.Write(testKey, testValue);
            
        // Assert
        Assert.IsTrue(storage.HasKey<string>(testKey));
        Assert.AreEqual(testValue, storage.Read<string>(testKey));
    }
    
    
    [Test]
    public void Write_WithIntValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "int_key";
        int    value = 42;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<int>(key));
        Assert.AreEqual(value, storage.Read<int>(key));
    }

    [Test]
    public void Write_WithFloatValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "float_key";
        float  value = 3.14f;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<float>(key));
        Assert.AreEqual(value, storage.Read<float>(key));
    }

    [Test]
    public void Write_WithDoubleValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "double_key";
        double value = 3.14159;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<double>(key));
        Assert.AreEqual(value, storage.Read<double>(key));
    }

    [Test]
    public void Write_WithCharValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "char_key";
        char   value = 'a';

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<char>(key));
        Assert.AreEqual(value, storage.Read<char>(key));
    }

    [Test]
    public void Write_WithBoolValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "bool_key";
        bool   value = true;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<bool>(key));
        Assert.AreEqual(value, storage.Read<bool>(key));
    }

    [Test]
    public void Write_WithLongValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "long_key";
        long   value = 123456789;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<long>(key));
        Assert.AreEqual(value, storage.Read<long>(key));
    }

    [Test]
    public void Write_WithShortValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "short_key";
        short  value = 123;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<short>(key));
        Assert.AreEqual(value, storage.Read<short>(key));
    }

    [Test]
    public void Write_WithByteValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "byte_key";
        byte   value = 255;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<byte>(key));
        Assert.AreEqual(value, storage.Read<byte>(key));
    }

    [Test]
    public void Write_WithSByteValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "sbyte_key";
        sbyte  value = -128;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<sbyte>(key));
        Assert.AreEqual(value, storage.Read<sbyte>(key));
    }

    [Test]
    public void Write_WithUIntValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "uint_key";
        uint   value = 123456789;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<uint>(key));
        Assert.AreEqual(value, storage.Read<uint>(key));
    }

    [Test]
    public void Write_WithULongValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "ulong_key";
        ulong  value = 123456789;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<ulong>(key));
        Assert.AreEqual(value, storage.Read<ulong>(key));
    }

    [Test]
    public void Write_WithUShortValue_StoresValueCorrectly()
    {
        // Arrange
        string key   = "ushort_key";
        ushort value = 50000;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<ushort>(key));
        Assert.AreEqual(value, storage.Read<ushort>(key));
    }

    [Test]
    public void Write_WithDecimalValue_StoresValueCorrectly()
    {
        // Arrange
        string  key   = "decimal_key";
        decimal value = 12345.6789m;

        // Act
        storage.Write(key, value);

        // Assert
        Assert.IsTrue(storage.HasKey<decimal>(key));
        Assert.AreEqual(value, storage.Read<decimal>(key));
    }
        
    [Test]
    public void Read_NonExistentKey_ReturnsDefault()
    {
        // Act
        string result = storage.Read<string>("non_existent");
            
        // Assert
        Assert.IsNull(result);
    }
        
    [Test]
    public void Read_NonExistentType_ReturnsDefault()
    {
        // Act
        int result = storage.Read<int>();
            
        // Assert
        Assert.AreEqual(0, result);
    }
        
    [Test]
    public void HasKey_ExistingKey_ReturnsTrue()
    {
        // Arrange
        storage.Write("existing_key", "value");
            
        // Act & Assert
        Assert.IsTrue(storage.HasKey<string>("existing_key"));
    }
        
    [Test]
    public void HasKey_NonExistentKey_ReturnsFalse()
    {
        // Act & Assert
        Assert.IsFalse(storage.HasKey<string>("non_existent_key"));
    }
        
    [Test]
    public void GetSavedKeys_ReturnsAllSavedKeys()
    {
        // Arrange
        storage.Write("key1", "value1");
        storage.Write("key2", "value2");
        storage.Write("key3", 42);
            
        // Act
        List<string> savedKeys = storage.GetSavedKeys();
            
        // Assert
        Assert.AreEqual(3, savedKeys.Count);
        Assert.Contains("checkout_key1", savedKeys);
        Assert.Contains("checkout_key2", savedKeys);
        Assert.Contains("checkout_key3", savedKeys);
    }
        
    [Test]
    public void ClearAll_RemovesAllData()
    {
        // Arrange
        storage.Write("key1", "value1");
        storage.Write("key2", "value2");
        Assert.AreEqual(2, storage.GetSavedKeys().Count);
            
        // Act
        storage.ClearAll();
            
        // Assert
        Assert.AreEqual(0, storage.GetSavedKeys().Count);
        Assert.IsFalse(storage.HasKey<string>("key1"));
        Assert.IsFalse(storage.HasKey<string>("key2"));
    }
        
    [Test]
    public void Write_DuplicateKey_OverwritesValue()
    {
        // Arrange
        string key = "duplicate_key";
        storage.Write(key, "original_value");
            
        // Act
        storage.Write(key, "updated_value");
            
        // Assert
        Assert.AreEqual("updated_value", storage.Read<string>(key));
        Assert.AreEqual(1,               storage.GetSavedKeys().Count); // Should not create duplicate key entries
    }
        
    [Test]
    public void Write_ComplexObject_SerializesCorrectly()
    {
        // Arrange
        var testObject = new TestData
                         {
                         Name     = "TestUser",
                         Score    = 100,
                         IsActive = true
                         };
            
        // Act
        storage.Write("complex_object", testObject);
        var retrievedObject = storage.Read<TestData>("complex_object");
            
        // Assert
        Assert.IsNotNull(retrievedObject);
        Assert.AreEqual(testObject.Name,     retrievedObject.Name);
        Assert.AreEqual(testObject.Score,    retrievedObject.Score);
        Assert.AreEqual(testObject.IsActive, retrievedObject.IsActive);
    }
        
    [Test]
    public void Write_AfterClear_WorksCorrectly()
    {
        // Arrange
        storage.Write("original_key", "original_value");
        storage.ClearAll();
            
        // Act
        storage.Write("new_key", "new_value");
            
        // Assert
        Assert.AreEqual(1,           storage.GetSavedKeys().Count);
        Assert.AreEqual("new_value", storage.Read<string>("new_key"));
        Assert.IsFalse(storage.HasKey<string>("original_key"));
    }
        
    [Test]
    public void GetSavedKeys_EmptyStorage_ReturnsEmptyList()
    {
        // Act
        List<string> keys = storage.GetSavedKeys();
            
        // Assert
        Assert.IsNotNull(keys);
        Assert.AreEqual(0, keys.Count);
    }
        
    [Test]
    public void Write_WithNullValue_HandlesCorrectly()
    {
        // Arrange
        string key = "null_key";
            
        // Act
        storage.Write(key, (string)null);
            
        // Assert
        Assert.IsTrue(storage.HasKey<string>(key));
        Assert.IsNull(storage.Read<string>(key));
    }
        
    [Test]
    public void Write_WithEmptyString_HandlesCorrectly()
    {
        // Arrange
        string key        = "empty_key";
        string emptyValue = "";
            
        // Act
        storage.Write(key, emptyValue);
            
        // Assert
        Assert.IsTrue(storage.HasKey<string>(key));
        Assert.AreEqual(emptyValue, storage.Read<string>(key));
    }
        
    // Test data class for complex object testing
    [System.Serializable]
    private class TestData
    {
        public string Name;
        public int    Score;
        public bool   IsActive;
    }
}