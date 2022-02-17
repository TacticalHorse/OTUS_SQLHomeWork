using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLHomeWork
{
    class BankBranch
    {
        public long Id { get; set; }
        public string BranchName { get; set; }
        public BankBranch(long Id, string Name)
        {
            this.Id = Id;
            this.BranchName = Name;
        }
    }
    class Person
    {
        public long Id { get; set; }
        public BankBranch BankBranch { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Balance { get; set; }
        public Person(long Id, BankBranch BankBranch, string Name, string Surname, double Balance)
        {
            this.Id = Id;
            this.BankBranch = BankBranch;
            this.Name = Name;
            this.Surname = Surname;
            this.Balance = Balance;
        }
    }
    class Transaction
    {
        public long Id { get; set; }
        public Person From { get; set; }
        public Person To { get; set; }
        public double Value { get; set; }
        public DateTime TimeStamp { get; set; }

        public Transaction(long Id, Person From, Person To, double Value, DateTime TimeStamp)
        {
            this.Id = Id;
            this.From = From;
            this.To = To;
            this.Value = Value;
            this.TimeStamp = TimeStamp;
        }
    }
}
