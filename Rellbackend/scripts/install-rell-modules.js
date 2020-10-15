const child_process = require('child_process');
const path = require('path');
const fs = require('fs-extra');
const os = require('os')

const config = require('./module.json');

function moduleCacheDirectory(moduleName) {
    return path.join(os.homedir(), '.rell', 'modules', moduleName);
}

function checkIfDirectoryExists(directoryName) {
    return fs.existsSync(directoryName)
}

function checkIfModuleExistsLocally(moduleName) {
    return checkIfDirectoryExists(moduleCacheDirectory(moduleName))
}

async function fetchModule(moduleName, repository, branch) {
    const { stdout, stderr } = await exec(`git clone --depth=1 --branch=${
        branch} ${repository
    } ${moduleCacheDirectory(moduleName)}`)
}

async function checkIfRefExistsLocally(moduleName, refName) {
    const refs = await fetchLocalRefs(moduleName);
    return refs.some(({ type, name }) => (name === refName))
}

async function fetchLocalRefs(moduleName) {
    const { stdout } = await exec(`git -C ${moduleCacheDirectory(moduleName)} show-ref`)

    return stdout
        .split('\n')
        .filter(Boolean)
        .map(ref => ref.split(' '))
        .map(([hash, ref]) => {
            if (ref === 'HEAD') {
                return {
                    type: 'HEAD',
                    hash
                };
            }

            const match = /refs\/(\w+)\/(.+)/.exec(ref);
            if (!match) {
                throw new Error('Error parsing refs');
            }

            return {
                type:
                    match[1] === 'heads'
                        ? 'branch'
                        : match[1] === 'refs'
                        ? 'ref'
                        : match[1],
                name: match[2],
                hash
            };
        });
}

async function checkIfRefExistsRemotely(moduleName, refName) {
    const refs = await fetchRemoteRefs(moduleName)
    return refs.some(({ type, name }) => (name === refName))
}

async function fetchRemoteRefs(moduleName, branch) {
    const { stdout } = await exec(`git -C ${moduleCacheDirectory(moduleName)} ls-remote`)
    
    return stdout
        .split('\n')
        .filter(Boolean)
        .map(ref => ref.split('\t'))
        .map(([hash, ref]) => {
            if (ref === 'HEAD') {
                return {
                    type: 'HEAD',
                    hash
                };
            }

            const match = /refs\/(\w+)\/(.+)/.exec(ref);
            if (!match) {
                throw new Error('Error parsign refs');
            }

            return {
                type:
                    match[1] === 'heads'
                        ? 'branch'
                        : match[1] === 'refs'
                        ? 'ref'
                        : match[1],
                name: match[2],
                hash
            };
        });
}

async function pullRemoteBranch(moduleName, branchName) {
    await exec(`git  -C ${moduleCacheDirectory(moduleName)} --depth=1 pull origin ${branchName}`)
}

async function checkoutBranch(moduleName, branchName) {
    await exec(`git  -C ${moduleCacheDirectory(moduleName)} checkout ${branchName}`)
}

async function copyModuleToProject(moduleName, moduleConfig, modulesDirectory) {
    const directory = modulesDirectory || "rell/src/rell_modules/"

    const sourcePath = path.join(moduleCacheDirectory(moduleName), moduleConfig.subdirectory || '')
    const destinationPath = path.join(directory, moduleName)

    if (await fs.pathExists(sourcePath) === false) {
        console.log(sourcePath)
        console.log(`Directory doesn't exist`)

        return
    }

    if (await fs.pathExists(destinationPath)) {
        fs.ensureDir(destinationPath)
    } else {
        //delete content
    }

    await fs.copy(sourcePath, destinationPath)
}

async function installModule(moduleName, moduleConfig, destinationDirectory) {
    const ref = moduleConfig.branch || "master"

    if (await checkIfModuleExistsLocally(moduleName)) {
        if (await checkIfRefExistsLocally(moduleName, ref)) {
            await checkoutBranch(moduleName, ref)
        } else if (await checkIfRefExistsRemotely(moduleName)) {
            await pullRemoteBranch(moduleName, moduleConfig.branch)
            await checkoutBranch(moduleName, ref)
        } else {
            //ref doesn't exist
        }
    } else {
        await fetchModule(moduleName, moduleConfig.repository, ref)
    }

    await copyModuleToProject(moduleName, moduleConfig, destinationDirectory);
}

async function installModules(config) {
    const destinationDirectory = config.directory || "rell/src/rell_modules/"

    for (const moduleName of Object.keys(config.dependencies)) {
        await installModule(moduleName, config.dependencies[moduleName], destinationDirectory);
    }
}

function exec(command) {
	return new Promise((resolve, reject) => {
		child_process.exec(command, (err, stdout, stderr) => {
			if (err) {
				reject(err);
				return;
			}

			resolve({ stdout, stderr });
		});
    });
}

(async () => {
    await installModules(config);
})()