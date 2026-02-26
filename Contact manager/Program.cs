using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;

namespace JsonExample
{
    public class Contact
    {
        private static int count = 1;

        public static void SetInitialCount(int value)
        {
            count = value;
        }

        public string Name { get; set; }
        public int Id { get; private set; }
        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public Contact(string name, string phone, string email)
        {
            Name = name;
            Phone = phone;
            Email = email;
            Id = count++;
            CreationDate = DateTime.Now;
        }

        // Required for JSON
        public Contact() { }
    }

    public class Manager
    {
        public List<Contact> Contacts { get; set; } = new List<Contact>();

        public void AddContact(Contact contact)
        {
            int index = 0;
            while (index < Contacts.Count &&
                   string.Compare(Contacts[index].Name, contact.Name) < 0)
            {
                index++;
            }

            Contacts.Insert(index, contact);
        }

        public void EditContact(int id)
        {
            Contact contact = Contacts.Find(c => c.Id == id);

            if (contact == null)
            {
                Console.WriteLine("Contact not found.");
                return;
            }

            contact.Name = Program.ReadValidatedInput("New Name: ", @"^[A-Za-z\s]{3,}$");
            contact.Phone = Program.ReadValidatedInput("New Phone (11 digits): ", @"^\d{11}$");
            contact.Email = Program.ReadValidatedInput("New Email: ", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            Console.WriteLine("Updated successfully.");
        }

        public void DeleteContact(int id)
        {
            Contact contact = Contacts.Find(c => c.Id == id);

            if (contact != null)
            {
                Contacts.Remove(contact);
                Console.WriteLine("Deleted successfully.");
            }
            else
            {
                Console.WriteLine("Contact not found.");
            }
        }

        public void ViewContact(int id)
        {
            Contact contact = Contacts.Find(c => c.Id == id);

            if (contact != null)
            {
                Console.WriteLine($"\nID: {contact.Id}");
                Console.WriteLine($"Name: {contact.Name}");
                Console.WriteLine($"Phone: {contact.Phone}");
                Console.WriteLine($"Email: {contact.Email}");
                Console.WriteLine($"Created: {contact.CreationDate}");
            }
            else
            {
                Console.WriteLine("Contact not found.");
            }
        }

        public void ListContacts()
        {
            if (Contacts.Count == 0)
            {
                Console.WriteLine("No contacts available.");
                return;
            }

            foreach (var contact in Contacts)
            {
                Console.WriteLine($"ID :{contact.Id} -Name :{contact.Name}");
            }
        }
    }

    public class Storage
    {
        private string path = "Records.json";

        public List<Contact> Load()
        {
            if (!File.Exists(path))
                return new List<Contact>();

            string json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<List<Contact>>(json)
                   ?? new List<Contact>();
        }

        public void Save(List<Contact> contacts)
        {
            string json = JsonSerializer.Serialize(
                contacts,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(path, json);
        }
    }

    class Program
    {
        public static string ReadValidatedInput(string message, string pattern)
        {
            string input;
            while (true)
            {
                Console.Write(message);
                input = Console.ReadLine();

                if (Regex.IsMatch(input, pattern))
                    return input;

                Console.WriteLine("Invalid input. Try again.");
            }
        }

 
        static void Main(string[] args)
        {
            Manager manager = new Manager();
            Storage storage = new Storage();

            manager.Contacts = storage.Load();

           
            if (manager.Contacts.Count > 0)
            {
                int maxId = -1;
                for (int i = 0; i < manager.Contacts.Count; i++)
                {
                    if (manager.Contacts[i].Id > maxId)
                    {
                        maxId = manager.Contacts[i].Id;
                    }
                }
                Contact.SetInitialCount(maxId + 1);
            }

            bool running = true;

            while (running)
            {
                Console.WriteLine("\n1) Add Contact");
                Console.WriteLine("2) Edit Contact");
                Console.WriteLine("3) Delete Contact");
                Console.WriteLine("4) View Contacts");
                Console.WriteLine("5) List all Contacts");
                Console.WriteLine("6) Save ");
                Console.WriteLine("7) Exit");

                string choice = ReadValidatedInput("Choose: ", @"^[1-7]$");

                switch (choice)
                {
                    case "1":
                        string name = ReadValidatedInput("Name: ", @"^[A-Za-z\s]{3,}$");
                        string phone = ReadValidatedInput("Phone (11 digits): ", @"^\d{11}$");
                        string email = ReadValidatedInput("Email: ", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                        Contact newContact = new Contact(name, phone, email);
                        manager.AddContact(newContact);
                        Console.WriteLine("Contact added successfully.");
                        break;

                    case "2":
                        manager.EditContact(int.Parse(ReadValidatedInput("Enter ID: ",@"^\d+$")));
                        break;

                    case "3":
                        manager.DeleteContact(int.Parse(ReadValidatedInput("Enter ID: ",@"^\d+$")));
                        break;

                    case "4":
                        manager.ViewContact(int.Parse(ReadValidatedInput("Enter ID: ",@"^\d+$")));
                        break;

                    case "5":
                        manager.ListContacts();
                        break;

                    case "6":
                        storage.Save(manager.Contacts);
                        Console.WriteLine("Saved successfully.");
                        break;

                    case "7":
                        running = false;
                        break;
                }
            }
        }
    }
}