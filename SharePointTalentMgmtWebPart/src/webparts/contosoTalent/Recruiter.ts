import { IconHelper } from "./utilities/IconHelper";
import { Position } from "./Position";

const MAX_HR_STUFF: number = 4;

export class Recruiter {
    public recruiterId: number;

    public name: string;

    public profilePicture: string;

    /**
     * Get all HR persons except Hiring managers.
     * @param allHrPersons Current list of persons from HR department.
     */
    public static GetHrStuff(currentPosition: Position): Recruiter[]{
        let result: Recruiter[] = [];
        currentPosition.candidates.forEach((c) => {
            c.interviews.forEach((i) => {
                if(result.filter(r => r.recruiterId == i.recruiterId).length === 0 && result.length < MAX_HR_STUFF){
                    result.push(i.recruiter);
                }
            });
        });

        return result;
    }

     /**
     * 
     * @param allHrPersons Build template for HR section.
     */
    public static GetHrTemplate(recruiters: Recruiter[]) : string {
        let hrStuffImagesTemplate: string = IconHelper.BuildIconTemplates(recruiters, true);
        let template: string = 
        `
        <div class="ms-sm12 ms-Grid-col">
            ${hrStuffImagesTemplate}
        </div>
        `;

        return template;
    }
}