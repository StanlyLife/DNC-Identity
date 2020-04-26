using Microsoft.AspNetCore.Identity;
using PluralsightIdentity.Models;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace PluralsightIdentity.Interfaces {

	public class MyUserStore : IUserStore<MyUser>, IUserPasswordStore<MyUser> {

		public async Task<IdentityResult> CreateAsync(MyUser user, CancellationToken cancellationToken) {
			using (var connection = GetOpenConnection()) {
				await connection.ExecuteAsync(
					"insert into DncIdentityUsers([Id]," +
					"[UserName]," +
					"[NormalizedUserName]," +
					"[PasswordHash]) " +
					"Values(@id,@userName,@normalizedUserName,@passwordHash)",
					new {
						id = user.Id,
						userName = user.UserName,
						normalizedUserName = user.NormalizedUsername,
						passwordHash = user.PasswordHash
					}
				);
			}

			return IdentityResult.Success;
		}

		public Task<IdentityResult> DeleteAsync(MyUser user, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		public void Dispose() {
		}

		public Task<MyUser> FindByIdAsync(string userId, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		public async Task<MyUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) {
			Console.WriteLine($"NormalizedUserName: {normalizedUserName}");
			using (var connection = GetOpenConnection()) {
				return await connection.QueryFirstOrDefaultAsync<MyUser>(
					"select * From DncIdentityUsers where NormalizedUserName = @name",
					new { name = normalizedUserName });
			}
		}

		public Task<string> GetNormalizedUserNameAsync(MyUser user, CancellationToken cancellationToken) {
			return Task.FromResult(user.NormalizedUsername);
		}

		public Task<string> GetUserIdAsync(MyUser user, CancellationToken cancellationToken) {
			return Task.FromResult(user.Id);
		}

		public Task<string> GetUserNameAsync(MyUser user, CancellationToken cancellationToken) {
			return Task.FromResult(user.UserName);
		}

		public Task SetNormalizedUserNameAsync(MyUser user, string normalizedName, CancellationToken cancellationToken) {
			user.UserName = normalizedName;
			return Task.CompletedTask;
		}

		public Task SetUserNameAsync(MyUser user, string userName, CancellationToken cancellationToken) {
			user.UserName = userName;
			return Task.CompletedTask;
		}

		public async Task<IdentityResult> UpdateAsync(MyUser user, CancellationToken cancellationToken) {
			using (var connection = GetOpenConnection()) {
				await connection.ExecuteAsync(
					"update DncIdentityUsers " +
					"set [Id] = @id," +
					"[UserName] = @userName," +
					"[NormalizedUserName] = @normalizedUserName," +
					"[PasswordHash] = @passwordHash " +
					"where [Id] = @id",
					new {
						id = user.Id,
						userName = user.UserName,
						normalizedUserName = user.NormalizedUsername,
						passwordHash = user.PasswordHash
					}
				);
			}

			return IdentityResult.Success;
		}

		public static DbConnection GetOpenConnection() {
			var connection = new SqlConnection("Data Source=(LocalDb)\\MSSQLLocalDB;" +
											   "database=DncIdentity;" +
											   "trusted_connection=yes;");
			connection.Open();
			return connection;
		}

		public Task SetPasswordHashAsync(MyUser user, string passwordHash, CancellationToken cancellationToken) {
			user.PasswordHash = passwordHash;
			return Task.CompletedTask;
		}

		public Task<string> GetPasswordHashAsync(MyUser user, CancellationToken cancellationToken) {
			return Task.FromResult(user.PasswordHash);
		}

		public Task<bool> HasPasswordAsync(MyUser user, CancellationToken cancellationToken) {
			return Task.FromResult(user.PasswordHash != null);
		}
	}
}