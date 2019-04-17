export class ApiService {
  public static GetServerData(serverUrl: string): string {
    let result: string = "";
    let xhr = new XMLHttpRequest();
    xhr.open('GET', serverUrl, false);
    xhr.onload = () => {
      if (xhr.status === 200) {
        result = xhr.responseText;
      }
    };

    xhr.send();

    return result;
  }

  public static UpdateServerData(serverUrl: string, object: any): void {
    let xhr = new XMLHttpRequest();
    xhr.open('PUT', serverUrl, true);
    xhr.setRequestHeader('Content-type','application/json; charset=utf-8');
    xhr.send(JSON.stringify(object));
  }
}