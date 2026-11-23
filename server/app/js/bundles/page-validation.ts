// Client-side form validation for the ASP.NET data-val-* attributes (shown by asp-validation-for / -summary), loaded
// by Pages/_ValidationScriptsPartial.cshtml. jQuery itself is the global from the `jquery` bundle (see the
// externals in webpack.config.ts); server-side validation still applies without this.
import "../jquery4-compat";
import "jquery-validation";
import "jquery-validation-unobtrusive";
