var mymodule=angular.modules("myModule", [])
    .controller("scmdashboard", function($scope,scmdatafromdb) {
        $scope.scmdata = "SCM Requests";
        scmdatafromdb.getscmdatafromdb().then(function (d) {
            $scope.datascm = d.data;
        })
    })
    .factory("scmservice", ["$http", function ($http) {
        var fac = {}
        fac.getscmdatafromdb = function (scm)
        {
            return $http.get("/Dashboard/ScmDashboardview");
            //$http.post("/Dashboard/ScmDashboardview",scm).success(function(response) {
            //    alert(response.status)
        }
    }])
;


