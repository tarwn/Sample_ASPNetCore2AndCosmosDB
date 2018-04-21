
export default SampleAppViewModel;

class SampleAppViewModel {
    constructor($) {
        this.isLoaded = false;
        this.rawMessage = null;
        this.computed = {
            message: () => {
                if(this.isLoaded){
                    return this.rawMessage;
                }
                else{
                    return "Loading...";
                }
            }
        };
    }
    
    initialize(){
        return this.getUserDetails();
    }

    getUserDetails() {
        // pretend AJAX call
        new Promise((resolve) => {
            console.log('starting');
            window.setTimeout(resolve, 2000);
        })
        .then(() => {
            console.log('next');
            this.rawMessage = "Loaded!";
            this.isLoaded = true;
        })
    }
}