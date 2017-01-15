using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace VainBot
{
    public static class Extensions
    {
        public static async Task<string> GetValueAsync(this DbSet<KeyValue> dbSetKv, DbKey key)
        {
            var keyValue = await dbSetKv.FirstAsync(kv => kv.Key == key.ToString());
            return keyValue.Value;
        }

        public static async Task<string> GetCorrectPluralityAsync(this decimal num, VbContext context)
        {
            string msg;

            if ((int)num == num)
                msg = ((int)num).ToString();
            else
                msg = num.ToString();

            msg += " ";

            if (num == 1)
                msg += await context.KeyValues.GetValueAsync(DbKey.PointsNameSingular);
            else
                msg += await context.KeyValues.GetValueAsync(DbKey.PointsNamePlural);

            return msg;
        }
    }
}
