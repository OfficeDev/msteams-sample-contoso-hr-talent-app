import { IPerson } from "../interfaces/IPerson";
import styles from '../ContosoTalentWebPart.module.scss';

export class IconHelper {
    public static BuildIconTemplates(persons: IPerson[], addName: boolean = false): string {
        let results: string = "";
        persons.forEach((p: IPerson) => results += this.BuildIconTemplate(p, addName));
        return results;
    }
    
    public static BuildIconTemplate(person: IPerson, addName: boolean = false, centerImage: boolean = false): string {
        let nameAddition: string = "";
        let extraClass: string = "";
        if(addName){
            extraClass = `ms-sm4 ms-Grid-col`;
            nameAddition = `
                <div class="${styles.imageSignature}">
                    <span>${person.name.substr(0, person.name.indexOf(' '))}</span>
                </div>
            `;
        }
        
        return `
        <div class="${extraClass}">
            <div class="${centerImage ? styles.center : ''}">
                <div class="ms-Image ${styles.itemImage} ">
                    <img alt="${person.name}" class="ms-Image-image is-loaded ms-Image-image--cover ms-Image-image--portrait is-fadeIn" src="${person.profilePicture}" />>
                </div>
                ${nameAddition}
            </div>
        </div>
        `;
    }
}