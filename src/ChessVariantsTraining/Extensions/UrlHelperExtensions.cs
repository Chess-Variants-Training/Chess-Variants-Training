using Microsoft.AspNetCore.Mvc;

namespace ChessVariantsTraining.Extensions
{
    public static class UrlHelperExtensions
    {
        private static int assetVersion = -1;

        public static bool SetAssetVersionIfUnset(int v)
        {
            if (assetVersion != -1) return false;

            assetVersion = v;
            return true;
        }
        public static string ContentWithAssetVersion(this IUrlHelper helper, string contentPath)
        {
            return string.Format("{0}?av={1}", helper.Content(contentPath), assetVersion);
        }
    }
}
